export const money = value => new Intl.NumberFormat('es-PE', { style: 'currency', currency: 'PEN' }).format(Number(value) || 0);
export const round = value => Math.round((Number(value) + Number.EPSILON) * 100) / 100;
export const uid = () => crypto.randomUUID();
export function dayKey(value = new Date()) {
  if (!value || Number.isNaN(new Date(value).getTime())) return null;
  const parts = Object.fromEntries(new Intl.DateTimeFormat('en', { timeZone: 'America/Lima', year: 'numeric', month: '2-digit', day: '2-digit' }).formatToParts(new Date(value)).map(p => [p.type, p.value]));
  return `${parts.year}-${parts.month}-${parts.day}`;
}
export function shiftDay(key, days) {
  const date = new Date(`${key}T12:00:00-05:00`);
  date.setUTCDate(date.getUTCDate() + days);
  return dayKey(date);
}
export const dateTime = value => value ? new Intl.DateTimeFormat('es-PE', { timeZone: 'America/Lima', dateStyle: 'short', timeStyle: 'short' }).format(new Date(value)) : 'Fecha no registrada';
export function validateProduct(input) {
  if (!input.name?.trim() || !input.category?.trim() || !input.unit?.trim()) throw new Error('Completa nombre, categoría y unidad.');
  if (input.name.length > 160 || input.category.length > 80 || input.unit.length > 30) throw new Error('Uno de los textos supera el límite permitido.');
  for (const field of ['stock', 'minStock', 'cost', 'price']) {
    const value = Number(input[field]);
    if (!Number.isFinite(value) || value < 0 || value > 9999999999.99 || round(value) !== value) throw new Error('Usa números no negativos de hasta dos decimales.');
  }
  if (Number(input.price) <= 0) throw new Error('El precio debe ser mayor que cero.');
}
export function updateCart(cart, products, id, quantity) {
  if (quantity <= 0) return cart.filter(line => line.id !== id);
  const product = products.find(p => p.id === id);
  if (!product || !Number.isFinite(quantity) || round(quantity) !== quantity || quantity > product.stock) throw new Error('La cantidad supera el stock disponible.');
  return cart.some(line => line.id === id) ? cart.map(line => line.id === id ? { ...product, quantity } : line) : [...cart, { ...product, quantity }];
}
export function reportFor(sales, start, end) {
  const dated = sales.filter(sale => sale.status !== 'cancelled' && dayKey(sale.soldAt));
  const selected = dated.filter(sale => dayKey(sale.soldAt) >= start && dayKey(sale.soldAt) <= end);
  const days = Math.round((new Date(end) - new Date(start)) / 86400000) + 1;
  const previousEnd = shiftDay(start, -1), previousStart = shiftDay(start, -days);
  const previousTotal = dated.filter(s => dayKey(s.soldAt) >= previousStart && dayKey(s.soldAt) <= previousEnd).reduce((sum, s) => sum + s.total, 0);
  const total = round(selected.reduce((sum, s) => sum + s.total, 0));
  const ranking = new Map(), payments = new Map();
  let knownRevenue = 0, profit = 0, missingCost = 0;
  for (const sale of selected) {
    payments.set(sale.method, round((payments.get(sale.method) || 0) + sale.total));
    for (const line of sale.lines || []) {
      const value = ranking.get(line.productId) || { id: line.productId, name: line.name, quantity: 0, total: 0 };
      value.quantity = round(value.quantity + line.quantity); value.total = round(value.total + line.total);
      ranking.set(line.productId, value);
      if (line.unitCost == null) missingCost++;
      else { knownRevenue += line.total; profit += line.total - line.quantity * line.unitCost; }
    }
    if (!sale.lines?.length) missingCost++;
  }
  return {
    selected, total, units: round(selected.reduce((sum, s) => sum + s.items, 0)),
    ticket: selected.length ? round(total / selected.length) : 0,
    growth: previousTotal ? round((total / previousTotal - 1) * 100) : null,
    margin: knownRevenue ? round(profit / knownRevenue * 100) : null, missingCost,
    unknownDates: sales.filter(s => s.status !== 'cancelled' && !dayKey(s.soldAt)).length,
    ranking: [...ranking.values()].sort((a, b) => b.quantity - a.quantity),
    payments: ['Efectivo','Yape','Plin','Tarjeta'].map(method => ({ method, total: payments.get(method) || 0, percent: total ? round((payments.get(method) || 0) / total * 100) : 0 })),
  };
}
export function csvText(rows) {
  // Un nombre de producto no debe convertirse en una fórmula al abrir Excel.
  return '\ufeff' + rows.map(row => row.map(value => {
    let text = String(value ?? '');
    if (/^[\s\u0000-\u001f]*[=+@-]/.test(text) && typeof value !== 'number') text = "'" + text;
    return '"' + text.replaceAll('"', '""') + '"';
  }).join(',')).join('\r\n');
}
export function validateSettings(input) {
  if (!input.name?.trim() || input.name.length > 120) throw new Error('Indica un nombre comercial de hasta 120 caracteres.');
  if (input.ruc && !/^[0-9]{11}$/.test(input.ruc)) throw new Error('El RUC debe tener 11 dígitos o quedar vacío.');
}
export function validateDemoBackup(backup) {
  const fail = message => { throw new Error('Respaldo inválido: ' + message); };
  if (!backup || backup.version !== 2 || backup.project !== 'Bodega Norte' ||
      (backup.format && backup.format !== 'demo-v2') || !backup.data) fail('selecciona una copia de la demostración local.');
  const data = structuredClone(backup.data);
  for (const key of ['products','archivedProducts','sales','suppliers','movements','audit','members']) {
    if (key === 'archivedProducts' && !data[key]) data[key] = [];
    if (!Array.isArray(data[key]) || data[key].length > 10000) fail('la lista ' + key + ' no es válida.');
    if (data[key].some(row => !row || typeof row !== 'object' || Array.isArray(row))) fail('hay un registro no válido en ' + key + '.');
  }
  const text = (value, max, required = false) => typeof value === 'string' && value.length <= max && (!required || value.trim().length > 0);
  const number = (value, min = 0) => typeof value === 'number' && Number.isFinite(value) && value >= min && value <= 9999999999.99 && round(value) === value;
  const date = value => value === null || (typeof value === 'string' && !Number.isNaN(new Date(value).getTime()));
  const unique = (rows,key,label) => { if(new Set(rows.map(r=>r[key])).size!==rows.length)fail('hay identificadores repetidos en '+label+'.'); };
  unique([...data.products,...data.archivedProducts],'id','productos');
  const products = new Set([...data.products,...data.archivedProducts].map(p=>p.id));
  for (const p of [...data.products,...data.archivedProducts]) {
    if (!text(p.id,60,true) || !text(p.name,160,true) || !text(p.category,80,true) || !text(p.unit,30,true) || !text(p.supplier,120) ||
        !Number.isSafeInteger(p.version) || p.version < 1 || !date(p.updated)) fail('un producto contiene datos no válidos.');
    validateProduct(p);
  }
  unique(data.suppliers,'id','proveedores');
  for(const s of data.suppliers) if(!Number.isSafeInteger(s.id)||s.id<1||!text(s.name,120,true)||!text(s.contact,120)||!text(s.phone,30)) fail('un proveedor contiene datos no válidos.');
  unique(data.sales,'id','ventas');
  for(const s of data.sales) {
    if(!text(s.id,60,true)||!text(s.customer,120,true)||!text(s.createdBy,254,true)||!date(s.soldAt)||!['active','cancelled'].includes(s.status)||
       !['Efectivo','Yape','Plin','Tarjeta'].includes(s.method)||!number(s.total)||!number(s.items)||!Array.isArray(s.lines)||s.lines.length>200)fail('una venta contiene datos no válidos.');
    for(const l of s.lines)if(!l||!products.has(l.productId)||!text(l.name,160,true)||!number(l.quantity,0.01)||!number(l.unitPrice,0.01)||!number(l.total)||
       (l.unitCost!==null&&!number(l.unitCost))||l.total!==round(l.quantity*l.unitPrice))fail('una línea de venta contiene datos no válidos.');
    if(s.lines.length&&(s.total!==round(s.lines.reduce((sum,l)=>sum+l.total,0))||s.items!==round(s.lines.reduce((sum,l)=>sum+l.quantity,0))))fail('el total o las unidades de una venta no coinciden con su detalle.');
    if(s.status==='cancelled'&&!text(s.cancelReason,300,true))fail('una venta anulada requiere un motivo válido.');
  }
  validateSettings(data.settings || {});
  if(!text(data.settings.ruc,11)||!text(data.settings.address,200)||!text(data.settings.phone,30)||typeof data.settings.stockAlerts!=='boolean')fail('la configuración contiene datos no válidos.');
  for(const m of data.movements)if(!text(m.id,80,true)||!text(m.name,160,true)||!products.has(m.productId)||!number(m.balance)||typeof m.quantity!=='number'||!number(Math.abs(m.quantity))||!text(m.reason,300,true)||!text(m.actor,254,true)||!date(m.createdAt)||!['initial','sale','adjustment','cancellation'].includes(m.type))fail('un movimiento contiene datos no válidos.');
  for(const a of data.audit)if(!text(a.id,80,true)||!text(a.action,80,true)||!text(a.entity,80,true)||!text(a.entityId,100,true)||!text(a.actor,254,true)||!date(a.createdAt))fail('un registro de auditoría contiene datos no válidos.');
  for(const m of data.members)if(!text(m.userId,60,true)||!text(m.email,254,true)||!m.email.includes('@')||!['admin','cashier'].includes(m.role)||typeof m.active!=='boolean')fail('un permiso de demostración contiene datos no válidos.');
  return data;
}
