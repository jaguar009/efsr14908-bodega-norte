const apiMode = import.meta.env.VITE_API_MODE || 'demo';
const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL || '/api').replace(/\/$/, '');

export const isSqlServerMode = apiMode === 'sql';

async function request(path, options = {}) {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    headers: { 'Content-Type': 'application/json', ...(options.headers || {}) },
    ...options,
  });
  const body = await response.json().catch(() => ({}));
  if (!response.ok) throw new Error(body.message || `La API respondió con ${response.status}.`);
  return body;
}

export const inventoryApi = {
  getProducts: () => request('/products'),
  getSales: () => request('/sales'),
  saveProduct: (product) => request('/products', { method: 'POST', body: JSON.stringify(product) }),
  deleteProduct: (id) => request(`/products/${encodeURIComponent(id)}`, { method: 'DELETE' }),
  createSale: (sale) => request('/sales', { method: 'POST', body: JSON.stringify(sale) }),
};
