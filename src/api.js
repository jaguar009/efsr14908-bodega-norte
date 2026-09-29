import { supabase } from './supabase';

const apiMode = import.meta.env.VITE_API_MODE || 'demo';
const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL || '/api').replace(/\/$/, '');

export const isSupabaseMode = apiMode === 'supabase';

async function request(path, options = {}) {
  const headers = { 'Content-Type': 'application/json', ...(options.headers || {}) };
  if (isSupabaseMode) {
    if (!supabase) throw new Error('Configura Supabase Auth antes de conectar con la API.');
    const { data, error } = await supabase.auth.getSession();
    if (error) throw error;
    if (!data.session?.access_token) throw new Error('Tu sesión expiró. Inicia sesión nuevamente.');
    headers.Authorization = `Bearer ${data.session.access_token}`;
  }

  const response = await fetch(`${apiBaseUrl}${path}`, {
    headers,
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
