import { supabase } from './supabase';
import { demoApi } from './demoApi';
export const isSupabaseMode = import.meta.env.VITE_API_MODE === 'supabase';
const base = (import.meta.env.VITE_API_BASE_URL || '/api').replace(/\/$/, '');
async function request(path, options = {}) {
  if (!supabase) throw new Error('Supabase Auth no está configurado.');
  const { data, error } = await supabase.auth.getSession();
  if (error) throw error;
  if (!data.session?.access_token) throw new Error('Tu sesión expiró. Inicia sesión de nuevo.');
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), 90000);
  try {
    const response = await fetch(base + path, { ...options, signal: controller.signal, headers: { 'Content-Type':'application/json', Authorization:'Bearer ' + data.session.access_token, ...options.headers } });
    const body = response.status === 204 ? null : await response.json().catch(() => ({}));
    if (!response.ok) {
      const error = new Error(body?.message || body?.detail || (response.status === 403 ? 'Tu cuenta no tiene permisos para esta operación.' : 'El servicio respondió con ' + response.status + '.'));
      error.status = response.status; throw error;
    }
    return body;
  } catch (error) {
    if (error.name === 'AbortError') throw new Error('El servidor tardó en responder. Reintenta; la venta conserva su identificador para evitar duplicados.');
    throw error;
  } finally { clearTimeout(timeout); }
}
const json = (method, body) => ({ method, body:JSON.stringify(body) });
const remote = {
  getProfile: () => request('/me'), getProducts: () => request('/products'), getSales: () => request('/sales'),
  getSuppliers: () => request('/suppliers'), getSettings: () => request('/settings'), getMovements: () => request('/movements'),
  getAudit: () => request('/audit'), getMembers: () => request('/members'),
  saveProduct: (p,id) => request('/products' + (id ? '/' + encodeURIComponent(id) : ''),json(id ? 'PUT':'POST',p)),
  deleteProduct: id => request('/products/' + encodeURIComponent(id),{method:'DELETE'}),
  adjustStock: (id,input) => request('/products/' + encodeURIComponent(id) + '/stock',json('POST',input)),
  createSale: input => request('/sales',json('POST',input)),
  cancelSale: (id,reason) => request('/sales/' + encodeURIComponent(id) + '/cancel',json('POST',{reason})),
  saveSupplier: (input,id) => request('/suppliers' + (id ? '/' + id : ''),json(id ? 'PUT':'POST',input)),
  deleteSupplier: id => request('/suppliers/' + id,{method:'DELETE'}),
  saveSettings: input => request('/settings',json('PUT',input)), saveMember: input => request('/members',json('PUT',input)),
  backup: () => request('/backup'),
};
export const inventoryApi = isSupabaseMode ? remote : demoApi;
