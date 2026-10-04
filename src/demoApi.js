import { initialProducts, initialSales, suppliers } from './seed.js';
import { round, uid, validateProduct, validateSettings } from './domain.js';

const KEY = 'bodega-norte:v2:data';
const actor = () => ({ email: 'administrador@demo.local', role: localStorage.getItem('bodega-norte:demo-role') || 'admin' });
const defaults = { name: 'Bodega Norte', ruc: '', address: '', phone: '', stockAlerts: true };
const seedLines = [
  [['P-001',2],['P-006',2],['P-008',3]],
  [['P-002',2],['P-003',1],['P-009',3]],
  [['P-006',2],['P-007',1]],
  [['P-004',3],['P-005',1],['P-008',1]],
  [['P-007',3]],
  [['P-010',2],['P-011',2],['P-012',3]],
];
function parse(key) { try { return JSON.parse(localStorage.getItem(key)); } catch { return null; } }
function read() {
  const saved = parse(KEY);
  if (saved) return { archivedProducts: [], ...saved };
  const oldProducts = parse('bodega-norte:v1:products'), oldSales = parse('bodega-norte:v1:sales');
  const products = (oldProducts || initialProducts).map(p => ({ ...p, version: 1, updated: null }));
  const sales = oldSales ? oldSales.map(s => ({ ...s, id: s.id.replace(/^#/, ''), soldAt: null, lines: [], status: 'active', createdBy: 'Datos locales anteriores' })) :
    initialSales.map((s, index) => ({ ...s, id: s.id.replace(/^#/, ''), soldAt: new Date(Date.now() - (index + 1) * 40 * 60000).toISOString(), status: 'active', createdBy: 'Demostración', lines: seedLines[index].map(([id, quantity]) => {
      const p = initialProducts.find(p => p.id === id);
      return { productId: id, name: p.name, quantity, unitPrice: p.price, unitCost: p.cost, total: round(p.price * quantity) };
    }) }));
  const data = { products, archivedProducts: [], sales, suppliers: suppliers.map((s, index) => ({ ...s, id: index + 1 })), settings: defaults, movements: [], audit: [], members: [{ userId: 'demo', email: 'administrador@demo.local', role: 'admin', active: true }] };
  localStorage.setItem(KEY, JSON.stringify(data)); return data;
}
function audit(data, action, entity, entityId) { data.audit.unshift({ id: uid(), action, entity, entityId, actor: actor().email, createdAt: new Date().toISOString() }); }
function movement(data, product, quantity, type, reason) { data.movements.unshift({ id: uid(), productId: product.id, name: product.name, quantity, balance: product.stock, type, reason, actor: actor().email, createdAt: new Date().toISOString() }); }
function admin() { if (actor().role !== 'admin') throw new Error('Esta operación requiere permisos de administrador.'); }
async function mutate(action) {
  const run = () => { const data = read(); const result = action(data); localStorage.setItem(KEY, JSON.stringify(data)); return result; };
  return navigator.locks ? navigator.locks.request('bodega-norte-data', run) : run();
}
function requireProduct(data, id) { const product = data.products.find(p => p.id === id); if (!product) throw new Error('El producto ya no está disponible.'); return product; }
function version(product, expected) { if (product.version !== expected) throw new Error('El producto cambió. Actualiza la lista antes de continuar.'); }
export const demoApi = {
  getProfile: async () => ({ userId: 'demo', ...actor() }),
  getProducts: async () => read().products,
  getSales: async () => read().sales,
  getSuppliers: async () => { const d = read(); return d.suppliers.map(s => ({ ...s, products: d.products.filter(p => p.supplier === s.name).length })); },
  getSettings: async () => read().settings,
  getMovements: async () => read().movements,
  getAudit: async () => { admin(); return read().audit; },
  getMembers: async () => { admin(); return read().members; },
  saveProduct: (input, id) => mutate(data => {
    admin(); validateProduct(input);
    if (input.supplier && !data.suppliers.some(s => s.name === input.supplier)) throw new Error('El proveedor no existe.');
    const product = id ? requireProduct(data, id) : null;
    if (product) { version(product, input.expectedVersion); if (product.stock !== input.stock) throw new Error('El stock cambió; usa Ajustar stock.'); }
    const saved = { ...input, id: id || 'P-' + uid().replaceAll('-', '').slice(0,20), version: (product?.version || 0) + 1, updated: new Date().toISOString() };
    delete saved.expectedVersion;
    data.products = product ? data.products.map(p => p.id === id ? saved : p) : [...data.products, saved];
    if (!product) movement(data, saved, saved.stock, 'initial', 'Stock inicial');
    audit(data, product ? 'update' : 'create', 'product', saved.id); return saved;
  }),
  deleteProduct: id => mutate(data => { admin(); data.archivedProducts.push(requireProduct(data, id)); data.products = data.products.filter(p => p.id !== id); audit(data,'delete','product',id); }),
  adjustStock: (id, input) => mutate(data => {
    admin(); const p = requireProduct(data, id); version(p, input.expectedVersion);
    if (!Number.isFinite(input.quantity) || !input.quantity || round(input.quantity) !== input.quantity || !input.reason.trim()) throw new Error('Indica una cantidad de hasta dos decimales y un motivo.');
    const balance = round(p.stock + input.quantity);
    if (balance < 0 || balance > 9999999999.99) throw new Error('El ajuste dejaría un stock fuera del límite.');
    p.stock = balance; p.version++; p.updated = new Date().toISOString();
    movement(data,p,input.quantity,'adjustment',input.reason); audit(data,'adjust_stock','product',id); return p;
  }),
  createSale: input => mutate(data => {
    const fingerprint = JSON.stringify({ method: input.paymentMethod, items: input.items, customer: input.customer });
    const existing = data.sales.find(s => s.requestId === input.requestId);
    if (existing) { if (existing.fingerprint !== fingerprint) throw new Error('La clave ya pertenece a otra venta.'); return existing; }
    if (!input.requestId || !input.items.length || !['Efectivo','Yape','Plin','Tarjeta'].includes(input.paymentMethod)) throw new Error('La venta está incompleta.');
    const grouped = new Map();
    for (const item of input.items) {
      if (!Number.isFinite(item.quantity) || item.quantity <= 0 || round(item.quantity) !== item.quantity) throw new Error('Cantidad inválida.');
      grouped.set(item.productId, round((grouped.get(item.productId) || 0) + item.quantity));
    }
    const lines = [...grouped].map(([id, quantity]) => {
      const p = requireProduct(data,id);
      if (quantity > p.stock) throw new Error('Stock insuficiente para ' + p.name + '.');
      if (input.items.some(i => i.productId === id && i.expectedPrice != null && i.expectedPrice !== p.price)) throw new Error('El precio cambió. Actualiza el carrito.');
      return { productId: id, name: p.name, quantity, unitPrice: p.price, unitCost: p.cost, total: round(quantity * p.price) };
    });
    const sale = { id: 'V-' + uid().replaceAll('-',''), requestId: input.requestId, fingerprint, soldAt: new Date().toISOString(), status: 'active', createdBy: actor().email, customer: input.customer || 'Venta mostrador', method: input.paymentMethod, lines, items: round(lines.reduce((sum,l) => sum + l.quantity,0)), total: round(lines.reduce((sum,l) => sum + l.total,0)) };
    for (const line of lines) { const p = requireProduct(data,line.productId); p.stock = round(p.stock-line.quantity); p.version++; movement(data,p,-line.quantity,'sale',sale.id); }
    data.sales.unshift(sale); audit(data,'create','sale',sale.id); return sale;
  }),
  cancelSale: (id, reason) => mutate(data => {
    admin(); const sale = data.sales.find(s => s.id === id);
    if (!sale) throw new Error('La venta no existe.');
    if (sale.status === 'cancelled') return sale;
    if (!reason.trim()) throw new Error('Indica el motivo de anulación.');
    if (!sale.lines?.length) throw new Error('Esta venta anterior no tiene detalle; no se puede restaurar su stock automáticamente.');
    const catalog = [...data.products,...data.archivedProducts];
    if (sale.lines.some(l => !catalog.some(p => p.id === l.productId))) throw new Error('Falta un producto del historial.');
    for (const line of sale.lines) { const p = catalog.find(p=>p.id===line.productId); p.stock = round(p.stock+line.quantity); p.version++; movement(data,p,line.quantity,'cancellation',reason); }
    sale.status = 'cancelled'; sale.cancelReason = reason; audit(data,'cancel','sale',id); return sale;
  }),
  saveSupplier: (input, id) => mutate(data => {
    admin(); if (!input.name.trim()) throw new Error('El nombre es obligatorio.');
    if (data.suppliers.some(s => s.name === input.name.trim() && s.id !== id)) throw new Error('Ya existe ese proveedor.');
    if (id) { const old = data.suppliers.find(s => s.id === id); if (!old) throw new Error('El proveedor no existe.'); for (const p of data.products) if (p.supplier === old.name) p.supplier = input.name.trim(); Object.assign(old,input,{ name: input.name.trim() }); }
    else data.suppliers.push({ ...input, name: input.name.trim(), id: Math.max(0,...data.suppliers.map(s => s.id))+1 });
    audit(data,'save','supplier',String(id || input.name));
  }),
  deleteSupplier: id => mutate(data => { admin(); const s = data.suppliers.find(s => s.id === id); if (!s) throw new Error('El proveedor no existe.'); if (data.products.some(p => p.supplier === s.name)) throw new Error('Reasigna sus productos antes de eliminar el proveedor.'); data.suppliers = data.suppliers.filter(s => s.id !== id); audit(data,'delete','supplier',String(id)); }),
  saveSettings: settings => mutate(data => { admin(); validateSettings(settings); data.settings = { ...settings }; audit(data,'update','settings','1'); }),
  saveMember: input => mutate(data => { admin(); if (!input.email.includes('@')) throw new Error('Correo inválido.'); if (input.email === actor().email) throw new Error('No puedes cambiar tus propios permisos.'); const old=data.members.find(m => m.email===input.email); if(old) Object.assign(old,input); else data.members.push({ ...input,userId:uid() }); audit(data,'permissions','member',input.email); }),
  backup: async () => { admin(); return { version:2,project:'Bodega Norte',exportedAt:new Date().toISOString(),data:read() }; },
};
