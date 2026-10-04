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
    if (/^[=+@-]/.test(text) && typeof value !== 'number') text = "'" + text;
    return '"' + text.replaceAll('"', '""') + '"';
  }).join(',')).join('\r\n');
}
export function validateSettings(input) {
  if (!input.name?.trim() || input.name.length > 120) throw new Error('Indica un nombre comercial de hasta 120 caracteres.');
  if (input.ruc && !/^[0-9]{11}$/.test(input.ruc)) throw new Error('El RUC debe tener 11 dígitos o quedar vacío.');
}
