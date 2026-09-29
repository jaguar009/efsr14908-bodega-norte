import React, { useEffect, useMemo, useState } from 'react';
import { createRoot } from 'react-dom/client';
import { HashRouter, Navigate, Route, Routes, useLocation, useNavigate } from 'react-router-dom';
import './styles.css';
import { inventoryApi, isSupabaseMode } from './api';
import { AuthPage, AuthProvider, ProtectedRoute, useAuth } from './auth';
import { isSupabaseConfigured } from './supabase';

const initialProducts = [
  { id: 'P-001', name: 'Leche Gloria 1L', category: 'Lácteos', stock: 3, minStock: 10, cost: 3.25, price: 4.5, supplier: 'Distribuidora Central', unit: 'unidad', updated: 'Hoy, 08:42' },
  { id: 'P-002', name: 'Pan de molde Bimbo', category: 'Panadería', stock: 5, minStock: 10, cost: 6.2, price: 8.5, supplier: 'Alimentos del Valle', unit: 'unidad', updated: 'Ayer, 17:30' },
  { id: 'P-003', name: 'Aceite Primor 1L', category: 'Abarrotes', stock: 4, minStock: 8, cost: 7.8, price: 10.5, supplier: 'Distribuidora Central', unit: 'unidad', updated: 'Hoy, 09:10' },
  { id: 'P-004', name: 'Azúcar Rubia 1kg', category: 'Abarrotes', stock: 6, minStock: 10, cost: 3.1, price: 4.2, supplier: 'Comercial San Luis', unit: 'unidad', updated: 'Ayer, 16:05' },
  { id: 'P-005', name: 'Arroz Costeño 5kg', category: 'Abarrotes', stock: 2, minStock: 8, cost: 17.5, price: 22.9, supplier: 'Comercial San Luis', unit: 'unidad', updated: 'Hoy, 07:56' },
  { id: 'P-006', name: 'Gaseosa Inca Kola 1.5L', category: 'Bebidas', stock: 24, minStock: 12, cost: 5.8, price: 8.5, supplier: 'Bebidas Andinas', unit: 'unidad', updated: 'Hoy, 10:18' },
  { id: 'P-007', name: 'Agua San Luis 625ml', category: 'Bebidas', stock: 32, minStock: 15, cost: 1.5, price: 2.5, supplier: 'Bebidas Andinas', unit: 'unidad', updated: 'Hoy, 10:04' },
  { id: 'P-008', name: 'Galletas Casino Chocolate', category: 'Snacks', stock: 18, minStock: 10, cost: 1.2, price: 2.0, supplier: 'Alimentos del Valle', unit: 'paquete', updated: 'Ayer, 18:10' },
  { id: 'P-009', name: 'Atún Florida 170g', category: 'Enlatados', stock: 11, minStock: 6, cost: 4.2, price: 6.2, supplier: 'Distribuidora Central', unit: 'lata', updated: 'Ayer, 14:12' },
  { id: 'P-010', name: 'Huevos Pardos x 15', category: 'Lácteos', stock: 9, minStock: 6, cost: 8.8, price: 11.9, supplier: 'Granja Santa Rosa', unit: 'bandeja', updated: 'Hoy, 08:10' },
  { id: 'P-011', name: 'Detergente Bolívar 500g', category: 'Limpieza', stock: 14, minStock: 8, cost: 4.4, price: 6.3, supplier: 'Comercial San Luis', unit: 'unidad', updated: 'Ayer, 12:48' },
  { id: 'P-012', name: 'Papel higiénico Elite x 4', category: 'Limpieza', stock: 7, minStock: 6, cost: 7.1, price: 9.9, supplier: 'Alimentos del Valle', unit: 'paquete', updated: 'Hoy, 09:22' },
];

const initialSales = [
  { id: '#1048', time: '10:24', customer: 'Venta mostrador', items: 7, total: 32.0, method: 'Yape' },
  { id: '#1047', time: '09:56', customer: 'Venta mostrador', items: 6, total: 46.1, method: 'Efectivo' },
  { id: '#1046', time: '09:12', customer: 'Venta mostrador', items: 3, total: 19.5, method: 'Efectivo' },
  { id: '#1045', time: '08:47', customer: 'Venta mostrador', items: 5, total: 37.5, method: 'Plin' },
  { id: '#1044', time: '08:21', customer: 'Venta mostrador', items: 1, total: 7.5, method: 'Efectivo' },
  { id: '#1043', time: '07:58', customer: 'Venta mostrador', items: 7, total: 66.1, method: 'Tarjeta' },
];

const suppliers = [
  { name: 'Distribuidora Central', contact: 'María Paredes', phone: '987 654 321', products: 4, status: 'Activo' },
  { name: 'Alimentos del Valle', contact: 'Luis Mendoza', phone: '986 212 480', products: 3, status: 'Activo' },
  { name: 'Comercial San Luis', contact: 'Rosa Salazar', phone: '985 730 118', products: 3, status: 'Activo' },
  { name: 'Bebidas Andinas', contact: 'Carlos Ruiz', phone: '982 440 607', products: 2, status: 'Activo' },
  { name: 'Granja Santa Rosa', contact: 'Ana Torres', phone: '981 330 929', products: 1, status: 'Activo' },
];

const navItems = [
  { id: 'dashboard', label: 'Inicio', icon: 'home' },
  { id: 'sales', label: 'Ventas', icon: 'cart' },
  { id: 'inventory', label: 'Inventario', icon: 'box' },
  { id: 'products', label: 'Productos', icon: 'tag' },
  { id: 'suppliers', label: 'Proveedores', icon: 'users' },
  { id: 'reports', label: 'Reportes', icon: 'chart' },
  { id: 'settings', label: 'Configuración', icon: 'settings' },
];

function Icon({ name, size = 20, stroke = 1.9 }) {
  const common = { width: size, height: size, viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', strokeWidth: stroke, strokeLinecap: 'round', strokeLinejoin: 'round', 'aria-hidden': true };
  const paths = {
    home: <><path d="m3 10 9-7 9 7" /><path d="M5 9.8V21h14V9.8" /><path d="M9 21v-6h6v6" /></>,
    cart: <><circle cx="9" cy="20" r="1.4" /><circle cx="18" cy="20" r="1.4" /><path d="M3 4h2l2.2 11.3a2 2 0 0 0 2 1.6h8.9a2 2 0 0 0 1.9-1.4L21 8H6" /></>,
    box: <><path d="m21 8-9 5-9-5 9-5 9 5Z" /><path d="M3 8v9l9 5 9-5V8" /><path d="M12 13v9" /></>,
    tag: <><path d="M20.6 13.4 13.4 20.6a2 2 0 0 1-2.8 0L3.4 13.4a2 2 0 0 1 0-2.8l7.2-7.2A2 2 0 0 1 12 2.8H19a2 2 0 0 1 2 2V12a2 2 0 0 1-.4 1.4Z" /><circle cx="16.2" cy="7.8" r="1.1" /></>,
    users: <><path d="M16 21v-1.7a3.3 3.3 0 0 0-3.3-3.3H6.3A3.3 3.3 0 0 0 3 19.3V21" /><circle cx="9.5" cy="8" r="3.2" /><path d="M17 11a3 3 0 1 0-1.1-5.8M21 21v-1.7a3.3 3.3 0 0 0-2.5-3.2" /></>,
    chart: <><path d="M4 20V10" /><path d="M10 20V4" /><path d="M16 20v-7" /><path d="M22 20H2" /></>,
    settings: <><circle cx="12" cy="12" r="3" /><path d="M19.4 15a1.7 1.7 0 0 0 .3 1.9l.1.1-1.8 1.8-.1-.1a1.7 1.7 0 0 0-1.9-.3 1.7 1.7 0 0 0-1 1.6v.2h-2.5V20a1.7 1.7 0 0 0-1-1.6 1.7 1.7 0 0 0-1.9.3l-.1.1-1.8-1.8.1-.1A1.7 1.7 0 0 0 8 15a1.7 1.7 0 0 0-1.6-1H6v-2.5h.4A1.7 1.7 0 0 0 8 10a1.7 1.7 0 0 0-.3-1.9l-.1-.1 1.8-1.8.1.1a1.7 1.7 0 0 0 1.9.3 1.7 1.7 0 0 0 1-1.6v-.2h2.5V5a1.7 1.7 0 0 0 1 1.6 1.7 1.7 0 0 0 1.9-.3l.1-.1 1.8 1.8-.1.1A1.7 1.7 0 0 0 19.4 10a1.7 1.7 0 0 0 1.6 1h.2v2.5H21a1.7 1.7 0 0 0-1.6 1.5Z" /></>,
    plus: <><path d="M12 5v14M5 12h14" /></>,
    search: <><circle cx="10.8" cy="10.8" r="6.5" /><path d="m16 16 4.5 4.5" /></>,
    bell: <><path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9" /><path d="M10 21h4" /></>,
    arrow: <><path d="M5 12h14" /><path d="m13 6 6 6-6 6" /></>,
    chevron: <path d="m8 10 4 4 4-4" />,
    download: <><path d="M12 3v12" /><path d="m7 10 5 5 5-5" /><path d="M5 21h14" /></>,
    edit: <><path d="M12 20h9" /><path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L8 18l-4 1 1-4Z" /></>,
    trash: <><path d="M4 7h16" /><path d="M10 11v6M14 11v6" /><path d="M6 7l1 14h10l1-14M9 7V4h6v3" /></>,
    close: <><path d="m6 6 12 12M18 6 6 18" /></>,
    filter: <><path d="M4 6h16M7 12h10M10 18h4" /></>,
    warning: <><path d="m12 3 9 17H3L12 3Z" /><path d="M12 9v4M12 17h.01" /></>,
    receipt: <><path d="M6 3h12v18l-3-2-3 2-3-2-3 2V3Z" /><path d="M9 8h6M9 12h6M9 16h3" /></>,
    help: <><circle cx="12" cy="12" r="9" /><path d="M9.8 9a2.4 2.4 0 1 1 4.1 1.7c-.9.8-1.9 1.2-1.9 2.8" /><path d="M12 17h.01" /></>,
    user: <><circle cx="12" cy="8" r="3" /><path d="M5 21a7 7 0 0 1 14 0" /></>,
    check: <path d="m5 12 4 4L19 6" />,
    menu: <><path d="M4 6h16M4 12h16M4 18h16" /></>,
    moon: <path d="M20 15.6A8.5 8.5 0 0 1 8.4 4 8.5 8.5 0 1 0 20 15.6Z" />,
  };
  return <svg {...common}>{paths[name]}</svg>;
}

const money = (value) => `S/ ${value.toFixed(2)}`;
const dateLabel = 'Lunes 14 de septiembre de 2026';
const readStored = (key, fallback) => {
  try {
    const value = window.localStorage.getItem(`bodega-norte:v1:${key}`);
    return value ? JSON.parse(value) : fallback;
  } catch {
    return fallback;
  }
};

function App() {
  const location = useLocation();
  const navigate = useNavigate();
  const { user, signOut, enabled: authEnabled } = useAuth();
  const requestedPage = location.pathname.split('/')[2] || 'dashboard';
  const page = navItems.some((item) => item.id === requestedPage) ? requestedPage : 'dashboard';
  const [products, setProducts] = useState(() => isSupabaseMode ? [] : readStored('products', initialProducts));
  const [sales, setSales] = useState(() => isSupabaseMode ? [] : readStored('sales', initialSales));
  const [query, setQuery] = useState('');
  const [cart, setCart] = useState([]);
  const [modal, setModal] = useState(null);
  const [toast, setToast] = useState('');
  const [dark, setDark] = useState(false);
  const [mobileNav, setMobileNav] = useState(false);
  const [assistantOpen, setAssistantOpen] = useState(false);

  const showToast = (message) => {
    setToast(message);
    window.setTimeout(() => setToast(''), 2800);
  };

  useEffect(() => {
    if (!isSupabaseMode) return undefined;
    let active = true;
    Promise.all([inventoryApi.getProducts(), inventoryApi.getSales()])
      .then(([remoteProducts, remoteSales]) => {
        if (!active) return;
        setProducts(remoteProducts);
        setSales(remoteSales);
      })
      .catch((error) => showToast(`No se pudo conectar con Supabase: ${error.message}`));
    return () => { active = false; };
  }, []);

  useEffect(() => { if (!isSupabaseMode) window.localStorage.setItem('bodega-norte:v1:products', JSON.stringify(products)); }, [products]);
  useEffect(() => { if (!isSupabaseMode) window.localStorage.setItem('bodega-norte:v1:sales', JSON.stringify(sales)); }, [sales]);

  const lowStock = useMemo(() => products.filter((product) => product.stock <= product.minStock), [products]);
  const stockTotal = useMemo(() => products.reduce((sum, product) => sum + product.stock, 0), [products]);
  const todayTotal = useMemo(() => sales.reduce((sum, sale) => sum + sale.total, 0), [sales]);
  const filteredProducts = useMemo(() => products.filter((p) => `${p.name} ${p.category} ${p.id}`.toLowerCase().includes(query.toLowerCase())), [products, query]);

  const addToCart = (product) => {
    const current = cart.find((line) => line.id === product.id);
    if ((current?.quantity || 0) >= product.stock) {
      showToast('No hay más unidades disponibles de este producto.');
      return;
    }
    setCart(current ? cart.map((line) => line.id === product.id ? { ...line, quantity: line.quantity + 1 } : line) : [...cart, { ...product, quantity: 1 }]);
  };

  const updateCart = (id, quantity) => {
    if (quantity <= 0) setCart(cart.filter((line) => line.id !== id));
    else setCart(cart.map((line) => line.id === id ? { ...line, quantity } : line));
  };

  const cartTotal = cart.reduce((sum, line) => sum + line.price * line.quantity, 0);

  const completeSale = async (method) => {
    if (!cart.length) return;
    const saleItems = cart.map((line) => ({ productId: line.id, quantity: line.quantity }));
    if (isSupabaseMode) {
      try {
        const result = await inventoryApi.createSale({ paymentMethod: method, items: saleItems });
        setSales((current) => [result.sale, ...current]);
        if (result.products) setProducts(result.products);
      } catch (error) {
        showToast(`No se pudo registrar la venta: ${error.message}`);
        return;
      }
    } else {
      const nextSale = { id: `#${1049 + sales.length - initialSales.length}`, time: 'Ahora', customer: 'Venta mostrador', items: cart.reduce((sum, line) => sum + line.quantity, 0), total: cartTotal, method };
      setSales((current) => [nextSale, ...current]);
      setProducts((current) => current.map((product) => {
        const line = cart.find((item) => item.id === product.id);
        return line ? { ...product, stock: product.stock - line.quantity, updated: 'Ahora' } : product;
      }));
    }
    setCart([]);
    setModal(null);
    showToast(isSupabaseMode ? 'Venta registrada en Supabase.' : 'Venta registrada correctamente.');
  };

  const saveProduct = async (product) => {
    const normalized = { ...product, stock: Number(product.stock), minStock: Number(product.minStock), cost: Number(product.cost), price: Number(product.price), updated: 'Ahora' };
    if (isSupabaseMode) {
      try {
        const saved = await inventoryApi.saveProduct(normalized);
        setProducts((current) => current.some((item) => item.id === saved.id) ? current.map((item) => item.id === saved.id ? saved : item) : [saved, ...current]);
      } catch (error) {
        showToast(`No se pudo guardar el producto: ${error.message}`);
        return;
      }
    } else {
      setProducts((current) => current.some((item) => item.id === normalized.id) ? current.map((item) => item.id === normalized.id ? normalized : item) : [normalized, ...current]);
    }
    setModal(null);
    showToast(isSupabaseMode ? 'Producto guardado en Supabase.' : 'Producto guardado correctamente.');
  };

  const exportReport = () => {
    const header = ['Código', 'Producto', 'Categoría', 'Stock', 'Mínimo', 'Precio', 'Proveedor'];
    const rows = products.map((p) => [p.id, p.name, p.category, p.stock, p.minStock, p.price.toFixed(2), p.supplier]);
    const csv = [header, ...rows].map((row) => row.map((value) => `"${String(value).replaceAll('"', '""')}"`).join(',')).join('\n');
    const blob = new Blob([`\ufeff${csv}`], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = 'bodega-norte-inventario.csv';
    link.click();
    URL.revokeObjectURL(url);
    showToast('Reporte CSV descargado.');
  };

  const deleteProduct = async (id) => {
    if (isSupabaseMode) {
      try {
        await inventoryApi.deleteProduct(id);
      } catch (error) {
        showToast(`No se pudo eliminar el producto: ${error.message}`);
        return;
      }
    }
    setProducts((current) => current.filter((product) => product.id !== id));
    showToast(isSupabaseMode ? 'Producto eliminado de Supabase.' : 'Producto eliminado.');
  };

  const go = (target) => {
    navigate(target === 'dashboard' ? '/app' : `/app/${target}`);
    setMobileNav(false);
    setQuery('');
  };

  const handleSignOut = async () => {
    try {
      await signOut();
      navigate('/login');
    } catch {
      showToast('No se pudo cerrar la sesión. Inténtalo nuevamente.');
    }
  };

  return (
    <div className={dark ? 'app dark' : 'app'}>
      <aside className={mobileNav ? 'sidebar mobile-open' : 'sidebar'}>
        <div className="brand">
          <div className="brand-mark"><span></span><span></span><span></span></div>
          <div><div className="brand-name">Bodega Norte</div><div className="brand-tagline">Tu barrio, siempre contigo</div></div>
        </div>
        <nav className="nav" aria-label="Navegación principal">
          {navItems.map((item) => <button key={item.id} className={page === item.id ? 'nav-item active' : 'nav-item'} onClick={() => go(item.id)}><Icon name={item.icon} size={21} /><span>{item.label}</span>{item.id === 'inventory' && lowStock.length > 0 && <span className="nav-count">{lowStock.length}</span>}</button>)}
        </nav>
        <div className="sidebar-note">
          <div className="receipt-illustration"><Icon name="receipt" size={26} /></div>
          <strong>Buen trabajo hoy</strong>
          <span>Revisa tus productos con stock bajo antes de cerrar.</span>
        </div>
        <div className="sidebar-footer"><span className="status-dot"></span><span>{isSupabaseMode ? 'Supabase PostgreSQL' : 'Demo local'}</span></div>
      </aside>

      <main className="main">
        <header className="topbar">
          <button className="mobile-menu" onClick={() => setMobileNav(!mobileNav)} aria-label="Abrir menú"><Icon name="menu" /></button>
          <div className="search-shell"><Icon name="search" size={20} /><input value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Buscar productos, códigos o clientes..." aria-label="Buscar" />{query && <button className="clear-search" onClick={() => setQuery('')}><Icon name="close" size={16} /></button>}</div>
          <div className="topbar-actions"><button className="assistant-button" onClick={() => setAssistantOpen(true)}><Icon name="help" size={18} /><span>Asistente</span></button><button className="icon-button" aria-label="Notificaciones" onClick={() => showToast('No tienes nuevas notificaciones.')}><Icon name="bell" size={21} /><span className="notification-dot"></span></button><div className="signed-in-user"><span className="avatar">{(user?.email?.[0] || 'A').toUpperCase()}</span><span className="profile-copy"><strong>{user?.email?.split('@')[0] || 'Ana'}</strong><small>{user ? 'Sesión activa' : 'Cajera demo'}</small></span></div>{authEnabled && <button className="logout-button" onClick={handleSignOut}>Cerrar sesión</button>}</div>
        </header>

        <div className="content">
          {page === 'dashboard' && <Dashboard products={products} lowStock={lowStock} sales={sales} todayTotal={todayTotal} stockTotal={stockTotal} onNavigate={go} onNewSale={() => go('sales')} onAddProduct={() => setModal({ type: 'product', product: null })} />}
          {page === 'sales' && <Sales products={products} cart={cart} query={query} setQuery={setQuery} onAdd={addToCart} onUpdate={updateCart} total={cartTotal} onCheckout={() => setModal({ type: 'checkout' })} onNavigate={go} />}
          {page === 'inventory' && <Inventory products={filteredProducts} query={query} setQuery={setQuery} onNew={() => setModal({ type: 'product', product: null })} onEdit={(product) => setModal({ type: 'product', product })} onDelete={deleteProduct} onNavigate={go} />}
          {page === 'products' && <Products products={filteredProducts} query={query} setQuery={setQuery} onNew={() => setModal({ type: 'product', product: null })} onEdit={(product) => setModal({ type: 'product', product })} onDelete={deleteProduct} />}
          {page === 'suppliers' && <Suppliers />}
          {page === 'reports' && <Reports products={products} sales={sales} lowStock={lowStock} onExport={exportReport} />}
          {page === 'settings' && <Settings dark={dark} setDark={setDark} onSave={() => showToast('Configuración guardada.')} />}
        </div>
      </main>

      {modal?.type === 'product' && <ProductModal product={modal.product} onClose={() => setModal(null)} onSave={saveProduct} />}
      {modal?.type === 'checkout' && <CheckoutModal total={cartTotal} onClose={() => setModal(null)} onConfirm={completeSale} />}
      {assistantOpen && <AssistantModal products={products} sales={sales} onClose={() => setAssistantOpen(false)} />}
      {toast && <div className="toast"><span className="toast-icon"><Icon name="check" size={16} /></span>{toast}</div>}
    </div>
  );
}

function PageHeader({ eyebrow, title, description, actions }) {
  return <div className="page-header"><div><div className="page-eyebrow">{eyebrow}</div><h1>{title}</h1><p>{description}</p></div><div className="page-actions">{actions}</div></div>;
}

function Dashboard({ products, lowStock, sales, todayTotal, stockTotal, onNavigate, onNewSale, onAddProduct }) {
  const chart = [32, 40, 51, 45, 62, 55, 76];
  const total = todayTotal || 482.5;
  return <>
    <PageHeader eyebrow="Resumen del negocio" title="Buenos días, Ana" description={dateLabel} actions={<><button className="button secondary" onClick={onNewSale}><Icon name="cart" size={19} />Nueva venta</button><button className="button primary" onClick={onAddProduct}><Icon name="plus" size={19} />Agregar producto</button></>} />
    <section className="kpi-grid">
      <Kpi icon="cart" label="Ventas de hoy" value={money(total)} change="+12%" detail="vs. ayer" tone="blue" />
      <Kpi icon="box" label="Productos en stock" value={stockTotal.toLocaleString('es-PE')} change="+3%" detail="vs. semana anterior" tone="lime" />
      <Kpi icon="warning" label="Stock bajo" value={lowStock.length.toString()} change={`+${Math.max(lowStock.length - 8, 1)}`} detail="vs. semana anterior" tone="terra" negative />
      <Kpi icon="chart" label="Ticket promedio" value={money(total / Math.max(sales.length, 1))} change="+8%" detail="vs. ayer" tone="slate" />
    </section>
    <section className="dashboard-grid">
      <div className="panel chart-panel"><div className="panel-header"><div><h2>Ventas de la semana</h2><span className="panel-subtitle">Ingresos registrados en los últimos 7 días</span></div><select className="select compact"><option>Últimos 7 días</option><option>Este mes</option></select></div><div className="chart-wrap"><div className="chart-y"><span>800</span><span>600</span><span>400</span><span>200</span><span>0</span></div><div className="chart"><div className="chart-grid-lines"><i></i><i></i><i></i><i></i><i></i></div><div className="chart-area" style={{ clipPath: 'polygon(0 67%, 16% 58%, 32% 42%, 49% 49%, 65% 27%, 82% 37%, 100% 8%, 100% 100%, 0 100%)' }}></div><svg className="chart-line" viewBox="0 0 700 260" preserveAspectRatio="none"><polyline points="0,175 116,153 233,113 350,130 466,77 583,101 700,21" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round" />{chart.map((_, i) => <circle key={i} cx={i * 116.6} cy={[175, 153, 113, 130, 77, 101, 21][i]} r="5" fill="white" stroke="currentColor" strokeWidth="3" />)}</svg><div className="chart-labels"><span>Lun 7</span><span>Mar 8</span><span>Mié 9</span><span>Jue 10</span><span>Vie 11</span><span>Sáb 12</span><span>Dom 13</span></div><div className="chart-callout">S/ 682.40</div></div></div></div>
      <Activity sales={sales} onViewAll={() => onNavigate('reports')} />
    </section>
    <section className="panel table-panel"><div className="panel-header"><div><h2>Productos con stock bajo</h2><span className="panel-subtitle">Revisa estos productos antes de hacer tu próximo pedido</span></div><button className="text-button" onClick={() => onNavigate('inventory')}>Ver inventario <Icon name="arrow" size={16} /></button></div><LowStockTable products={lowStock.slice(0, 5)} /></section>
  </>;
}

function Kpi({ icon, label, value, change, detail, tone, negative }) { return <div className="kpi"><div className={`kpi-icon ${tone}`}><Icon name={icon} size={21} /></div><div className="kpi-body"><span>{label}</span><strong>{value}</strong><small className={negative ? 'negative' : ''}><b>{negative ? '↗' : '↗'} {change}</b> {detail}</small></div></div>; }

function Activity({ sales, onViewAll }) { return <div className="panel activity-panel"><div className="panel-header"><div><h2>Actividad reciente</h2><span className="panel-subtitle">Últimas ventas registradas</span></div><button className="text-button" onClick={onViewAll}>Ver todas <Icon name="arrow" size={16} /></button></div><div className="activity-list">{sales.slice(0, 6).map((sale) => <div className="activity-row" key={sale.id}><span className="activity-icon"><Icon name="cart" size={17} /></span><div className="activity-copy"><strong>Venta {sale.id}</strong><span>{sale.items} {sale.items === 1 ? 'producto' : 'productos'} · {sale.method}</span></div><time>{sale.time}</time><b>{money(sale.total)}</b></div>)}</div></div>; }

function LowStockTable({ products, full = false, onEdit }) { return <div className="table-scroll"><table><thead><tr><th>Producto</th><th>Categoría</th><th>Stock</th><th>Mínimo</th><th>Estado</th>{full && <th>Acciones</th>}</tr></thead><tbody>{products.map((p) => <tr key={p.id}><td><div className="product-cell"><span className="product-thumb">{p.name.slice(0, 1)}</span><div><strong>{p.name}</strong><small>{p.id}</small></div></div></td><td>{p.category}</td><td><strong className="stock-number">{p.stock}</strong> {p.unit}</td><td>{p.minStock}</td><td><span className="status alert">Stock bajo</span></td>{full && <td><button className="row-action" onClick={() => onEdit?.(p)}><Icon name="edit" size={16} /></button></td>}</tr>)}</tbody></table>{!products.length && <div className="empty-state">No hay productos en esta vista.</div>}</div>; }

function Sales({ products, cart, query, setQuery, onAdd, onUpdate, total, onCheckout, onNavigate }) {
  const [category, setCategory] = useState('Todos');
  const categories = ['Todos', ...new Set(products.map((p) => p.category))];
  const shown = products.filter((p) => (category === 'Todos' || p.category === category) && `${p.name} ${p.id}`.toLowerCase().includes(query.toLowerCase()));
  return <><PageHeader eyebrow="Punto de venta" title="Nueva venta" description="Registra una venta y actualiza tu inventario al instante." actions={<button className="button ghost" onClick={() => onNavigate('reports')}><Icon name="receipt" size={18} />Historial de ventas</button>} /><div className="sales-layout"><section className="panel catalog-panel"><div className="catalog-toolbar"><div className="inline-search"><Icon name="search" size={18} /><input value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Buscar producto..." /></div><div className="category-tabs">{categories.map((item) => <button key={item} className={category === item ? 'category-tab active' : 'category-tab'} onClick={() => setCategory(item)}>{item}</button>)}</div></div><div className="catalog-grid">{shown.map((product) => <button className="catalog-item" key={product.id} onClick={() => onAdd(product)} disabled={product.stock === 0}><div className={`catalog-color c-${product.category.toLowerCase().replace('á', 'a').replace('é', 'e')}`}><Icon name={product.category === 'Bebidas' ? 'receipt' : product.category === 'Limpieza' ? 'box' : 'tag'} size={24} /></div><div className="catalog-copy"><strong>{product.name}</strong><span>{product.category} · {product.stock} disponibles</span></div><b>{money(product.price)}</b></button>)}</div></section><aside className="panel cart-panel"><div className="cart-header"><div><h2>Venta actual</h2><span>{cart.reduce((sum, line) => sum + line.quantity, 0)} productos</span></div><button className="text-button muted" onClick={() => cart.forEach((line) => onUpdate(line.id, 0))} disabled={!cart.length}>Limpiar</button></div>{cart.length ? <div className="cart-lines">{cart.map((line) => <div className="cart-line" key={line.id}><div><strong>{line.name}</strong><span>{money(line.price)} c/u</span></div><div className="quantity"><button onClick={() => onUpdate(line.id, line.quantity - 1)}>-</button><span>{line.quantity}</span><button onClick={() => onUpdate(line.id, line.quantity + 1)}>+</button></div><b>{money(line.price * line.quantity)}</b></div>)}</div> : <div className="cart-empty"><span><Icon name="cart" size={28} /></span><strong>Tu venta está vacía</strong><p>Selecciona productos para empezar.</p></div>}<div className="cart-summary"><div><span>Subtotal</span><b>{money(total)}</b></div><div><span>IGV incluido</span><b>{money(total * 0.18)}</b></div><div className="cart-total"><span>Total</span><strong>{money(total)}</strong></div><button className="button primary wide" onClick={onCheckout} disabled={!cart.length}>Cobrar venta <Icon name="arrow" size={18} /></button></div></aside></div></>;
}

function Inventory({ products, query, setQuery, onNew, onEdit, onDelete, onNavigate }) { const [status, setStatus] = useState('Todos'); const filtered = products.filter((p) => status === 'Todos' || (status === 'Bajo' ? p.stock <= p.minStock : p.stock > p.minStock)); return <><PageHeader eyebrow="Control de existencias" title="Inventario" description={`${products.length} productos registrados · ${products.filter((p) => p.stock <= p.minStock).length} necesitan reposición`} actions={<><button className="button ghost" onClick={() => onNavigate('reports')}><Icon name="download" size={18} />Exportar</button><button className="button primary" onClick={onNew}><Icon name="plus" size={18} />Agregar producto</button></>} /><div className="panel inventory-panel"><div className="inventory-toolbar"><div className="inline-search"><Icon name="search" size={18} /><input value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Buscar por nombre, código o categoría..." /></div><div className="toolbar-right"><button className="filter-button"><Icon name="filter" size={17} />Filtrar</button><select className="select" value={status} onChange={(e) => setStatus(e.target.value)}><option>Todos</option><option>Disponible</option><option>Bajo</option></select></div></div><div className="inventory-meta"><span>Mostrando <b>{filtered.length}</b> productos</span><span className="legend"><i className="legend-dot green"></i>Disponible <i className="legend-dot red"></i>Stock bajo</span></div><div className="table-scroll"><table><thead><tr><th>Producto</th><th>Categoría</th><th>Stock actual</th><th>Precio venta</th><th>Proveedor</th><th>Estado</th><th></th></tr></thead><tbody>{filtered.map((p) => <tr key={p.id}><td><div className="product-cell"><span className="product-thumb">{p.name.slice(0, 1)}</span><div><strong>{p.name}</strong><small>{p.id}</small></div></div></td><td>{p.category}</td><td><strong className={p.stock <= p.minStock ? 'stock-number alert-text' : 'stock-number'}>{p.stock}</strong> <small>{p.unit}</small></td><td><strong>{money(p.price)}</strong></td><td>{p.supplier}</td><td><span className={p.stock <= p.minStock ? 'status alert' : 'status good'}>{p.stock <= p.minStock ? 'Stock bajo' : 'Disponible'}</span></td><td><div className="row-actions"><button className="row-action" onClick={() => onEdit(p)} aria-label={`Editar ${p.name}`}><Icon name="edit" size={16} /></button><button className="row-action danger" onClick={() => onDelete(p.id)} aria-label={`Eliminar ${p.name}`}><Icon name="trash" size={16} /></button></div></td></tr>)}</tbody></table></div></div></>; }

function Products({ products, query, setQuery, onNew, onEdit, onDelete }) { return <><PageHeader eyebrow="Catálogo" title="Productos" description="Administra los datos comerciales de los productos de la bodega." actions={<button className="button primary" onClick={onNew}><Icon name="plus" size={18} />Nuevo producto</button>} /><div className="product-cards">{products.slice(0, 3).map((p) => <div className="product-summary" key={p.id}><div className="summary-top"><span className={`catalog-color c-${p.category.toLowerCase().replace('á', 'a').replace('é', 'e')}`}><Icon name="tag" size={22} /></span><button className="more-button" onClick={() => onEdit(p)}>Editar</button></div><h3>{p.name}</h3><p>{p.category} · {p.id}</p><div className="summary-bottom"><span>Precio <b>{money(p.price)}</b></span><span>Stock <b className={p.stock <= p.minStock ? 'alert-text' : ''}>{p.stock}</b></span></div></div>)}</div><div className="panel"><div className="inventory-toolbar"><div><h2>Catálogo completo</h2><span className="panel-subtitle">Precios, unidades y proveedores</span></div><div className="inline-search short"><Icon name="search" size={18} /><input value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Buscar..." /></div></div><div className="table-scroll"><table><thead><tr><th>Producto</th><th>Categoría</th><th>Costo</th><th>Precio</th><th>Margen</th><th>Proveedor</th><th></th></tr></thead><tbody>{products.map((p) => <tr key={p.id}><td><div className="product-cell"><span className="product-thumb">{p.name.slice(0, 1)}</span><div><strong>{p.name}</strong><small>{p.id} · por {p.unit}</small></div></div></td><td>{p.category}</td><td>{money(p.cost)}</td><td><strong>{money(p.price)}</strong></td><td>{Math.round((1 - p.cost / p.price) * 100)}%</td><td>{p.supplier}</td><td><div className="row-actions"><button className="row-action" onClick={() => onEdit(p)}><Icon name="edit" size={16} /></button><button className="row-action danger" onClick={() => onDelete(p.id)}><Icon name="trash" size={16} /></button></div></td></tr>)}</tbody></table></div></div></>; }

function Suppliers() { return <><PageHeader eyebrow="Abastecimiento" title="Proveedores" description="Mantén a la mano tus contactos y el catálogo que te abastecen." actions={<button className="button primary"><Icon name="plus" size={18} />Nuevo proveedor</button>} /><div className="supplier-grid">{suppliers.map((supplier) => <div className="supplier-card" key={supplier.name}><div className="supplier-head"><span className="supplier-avatar">{supplier.name.slice(0, 1)}</span><span className="status good">{supplier.status}</span></div><h3>{supplier.name}</h3><p>{supplier.contact}</p><div className="supplier-details"><span>{supplier.phone}</span><span>{supplier.products} productos</span></div><button className="supplier-link">Ver productos <Icon name="arrow" size={16} /></button></div>)}</div><div className="panel supplier-table"><div className="panel-header"><div><h2>Resumen de abastecimiento</h2><span className="panel-subtitle">Proveedores activos y productos relacionados</span></div></div><table><thead><tr><th>Proveedor</th><th>Contacto</th><th>Teléfono</th><th>Productos</th><th>Estado</th></tr></thead><tbody>{suppliers.map((supplier) => <tr key={supplier.name}><td><strong>{supplier.name}</strong></td><td>{supplier.contact}</td><td>{supplier.phone}</td><td>{supplier.products}</td><td><span className="status good">Activo</span></td></tr>)}</tbody></table></div></>; }

function Reports({ products, sales, lowStock, onExport }) { const salesTotal = sales.reduce((sum, sale) => sum + sale.total, 0); const units = sales.reduce((sum, sale) => sum + sale.items, 0); return <><PageHeader eyebrow="Lectura del negocio" title="Reportes" description="Indicadores para decidir qué reponer y cómo se mueve tu bodega." actions={<><button className="button ghost" onClick={onExport}><Icon name="download" size={18} />Exportar CSV</button><select className="select"><option>Septiembre 2026</option><option>Agosto 2026</option></select></>} /><div className="report-summary"><div><span>Ventas acumuladas</span><strong>{money(salesTotal)}</strong><small>+12.4% vs. período anterior</small></div><div><span>Unidades vendidas</span><strong>{units}</strong><small>+8.1% vs. período anterior</small></div><div><span>Margen estimado</span><strong>26.8%</strong><small>Sobre productos registrados</small></div><div><span>Productos por reponer</span><strong className="alert-text">{lowStock.length}</strong><small>Requieren atención</small></div></div><div className="reports-grid"><div className="panel"><div className="panel-header"><div><h2>Productos más vendidos</h2><span className="panel-subtitle">Estimación por ventas registradas</span></div></div><div className="ranking">{products.slice(0, 5).map((p, index) => <div className="rank-row" key={p.id}><span className="rank-number">0{index + 1}</span><div className="rank-product"><strong>{p.name}</strong><span>{p.category}</span></div><div className="rank-bar"><i style={{ width: `${88 - index * 12}%` }}></i></div><b>{Math.max(18 - index * 2, 8)} und.</b></div>)}</div></div><div className="panel"><div className="panel-header"><div><h2>Ventas por medio de pago</h2><span className="panel-subtitle">Distribución del período</span></div></div><div className="payment-chart"><div className="donut"></div><div className="payment-legend"><span><i className="legend-dot blue"></i>Efectivo <b>42%</b></span><span><i className="legend-dot lime"></i>Yape <b>31%</b></span><span><i className="legend-dot terra"></i>Tarjeta <b>18%</b></span><span><i className="legend-dot slate"></i>Plin <b>9%</b></span></div></div></div></div><div className="panel"><div className="panel-header"><div><h2>Alertas de inventario</h2><span className="panel-subtitle">Productos por debajo del mínimo configurado</span></div></div><LowStockTable products={lowStock} /></div></>; }

function Settings({ dark, setDark, onSave }) { return <><PageHeader eyebrow="Preferencias" title="Configuración" description="Personaliza la operación de Bodega Norte." actions={<button className="button primary" onClick={onSave}><Icon name="check" size={18} />Guardar cambios</button>} /><div className="settings-layout"><div className="panel settings-menu"><button className="settings-tab active">Datos de la bodega</button><button className="settings-tab">Usuarios y permisos</button><button className="settings-tab">Notificaciones</button><button className="settings-tab">Respaldo de datos</button></div><div className="panel settings-form"><div className="panel-header"><div><h2>Datos de la bodega</h2><span className="panel-subtitle">Información que aparecerá en tus comprobantes</span></div></div><div className="form-grid"><label>Nombre comercial<input defaultValue="Bodega Norte" /></label><label>RUC<input defaultValue="20601234567" /></label><label>Dirección<input defaultValue="Av. Los Pinos 245, Lima" /></label><label>Teléfono<input defaultValue="01 456 7890" /></label></div><div className="setting-line"><div><strong>Modo oscuro</strong><span>Reduce el brillo de la pantalla durante turnos nocturnos.</span></div><button className={dark ? 'toggle on' : 'toggle'} onClick={() => setDark(!dark)} aria-label="Cambiar modo oscuro"><i></i></button></div><div className="setting-line"><div><strong>Alertas de stock bajo</strong><span>Mostrar recordatorios cuando un producto llegue al mínimo.</span></div><button className="toggle on"><i></i></button></div></div></div></>; }

function ProductModal({ product, onClose, onSave }) { const [form, setForm] = useState(product || { id: `P-${String(Math.floor(Math.random() * 900) + 100)}`, name: '', category: 'Abarrotes', stock: 0, minStock: 5, cost: 0, price: 0, supplier: 'Distribuidora Central', unit: 'unidad' }); const change = (key, value) => setForm({ ...form, [key]: value }); return <div className="modal-backdrop"><div className="modal"><div className="modal-header"><div><div className="page-eyebrow">Catálogo</div><h2>{product ? 'Editar producto' : 'Agregar producto'}</h2><p>Completa los datos para mantener tu inventario actualizado.</p></div><button className="close-button" onClick={onClose}><Icon name="close" /></button></div><div className="modal-form"><label>Nombre del producto<input autoFocus value={form.name} onChange={(e) => change('name', e.target.value)} placeholder="Ej. Fideos Don Vittorio 500g" /></label><div className="form-grid two"><label>Categoría<select value={form.category} onChange={(e) => change('category', e.target.value)}>{['Abarrotes', 'Bebidas', 'Lácteos', 'Panadería', 'Snacks', 'Limpieza', 'Enlatados'].map((c) => <option key={c}>{c}</option>)}</select></label><label>Unidad de venta<select value={form.unit} onChange={(e) => change('unit', e.target.value)}><option>unidad</option><option>paquete</option><option>lata</option><option>bandeja</option></select></label></div><div className="form-grid three"><label>Stock actual<input type="number" min="0" value={form.stock} onChange={(e) => change('stock', e.target.value)} /></label><label>Stock mínimo<input type="number" min="0" value={form.minStock} onChange={(e) => change('minStock', e.target.value)} /></label><label>Proveedor<select value={form.supplier} onChange={(e) => change('supplier', e.target.value)}>{suppliers.map((s) => <option key={s.name}>{s.name}</option>)}</select></label></div><div className="form-grid two"><label>Costo unitario (S/)<input type="number" min="0" step="0.1" value={form.cost} onChange={(e) => change('cost', e.target.value)} /></label><label>Precio de venta (S/)<input type="number" min="0" step="0.1" value={form.price} onChange={(e) => change('price', e.target.value)} /></label></div></div><div className="modal-footer"><button className="button ghost" onClick={onClose}>Cancelar</button><button className="button primary" onClick={() => onSave(form)} disabled={!form.name.trim() || Number(form.price) <= 0}>Guardar producto</button></div></div></div>; }

function CheckoutModal({ total, onClose, onConfirm }) { const [method, setMethod] = useState('Efectivo'); return <div className="modal-backdrop"><div className="modal checkout-modal"><div className="modal-header"><div><div className="page-eyebrow">Punto de venta</div><h2>Confirmar cobro</h2><p>Selecciona el medio de pago para cerrar la venta.</p></div><button className="close-button" onClick={onClose}><Icon name="close" /></button></div><div className="checkout-total"><span>Total a cobrar</span><strong>{money(total)}</strong></div><div className="payment-options">{['Efectivo', 'Yape', 'Plin', 'Tarjeta'].map((item) => <button key={item} className={method === item ? 'payment-option active' : 'payment-option'} onClick={() => setMethod(item)}><span className={`payment-mark ${item.toLowerCase()}`}>{item === 'Efectivo' ? 'S/' : item.slice(0, 1)}</span><strong>{item}</strong>{method === item && <Icon name="check" size={17} />}</button>)}</div><div className="modal-footer"><button className="button ghost" onClick={onClose}>Volver</button><button className="button primary" onClick={() => onConfirm(method)}>Registrar venta <Icon name="check" size={17} /></button></div></div></div>; }

function AssistantModal({ products, sales, onClose }) {
  const [question, setQuestion] = useState('');
  const [messages, setMessages] = useState([{ role: 'assistant', text: 'Hola, Ana. Puedo ayudarte a revisar el stock, las ventas o el uso del sistema.' }]);
  const low = products.filter((p) => p.stock <= p.minStock);
  const answer = (input) => {
    const normalized = input.toLowerCase();
    if (normalized.includes('stock') && (normalized.includes('bajo') || normalized.includes('reponer'))) return `Tienes ${low.length} productos por debajo del mínimo: ${low.slice(0, 3).map((p) => p.name).join(', ')}${low.length > 3 ? ' y otros.' : '.'}`;
    if (normalized.includes('venta') || normalized.includes('vend')) return `Hay ${sales.length} ventas registradas por ${money(sales.reduce((sum, sale) => sum + sale.total, 0))}. Puedes ver el detalle en Reportes.`;
    if (normalized.includes('producto') && (normalized.includes('agregar') || normalized.includes('crear'))) return 'Para agregar un producto, abre Productos o Inventario y pulsa Agregar producto.';
    if (normalized.includes('export')) return 'Abre Reportes y pulsa Exportar CSV para descargar el inventario actual.';
    return 'Puedo responder sobre stock bajo, ventas, productos y exportación. Prueba con una de las sugerencias.';
  };
  const submit = (value = question) => { if (!value.trim()) return; setMessages([...messages, { role: 'user', text: value }, { role: 'assistant', text: answer(value) }]); setQuestion(''); };
  return <div className="modal-backdrop"><div className="modal assistant-modal"><div className="modal-header"><div><div className="page-eyebrow">Ayuda operativa</div><h2>Asistente de bodega</h2><p>Respuestas rápidas para operar Bodega Norte.</p></div><button className="close-button" onClick={onClose}><Icon name="close" /></button></div><div className="assistant-body"><div className="assistant-messages">{messages.map((message, index) => <div className={message.role === 'assistant' ? 'assistant-message' : 'assistant-message user'} key={`${message.role}-${index}`}><span className="assistant-bubble-icon"><Icon name={message.role === 'assistant' ? 'help' : 'user'} size={15} /></span><p>{message.text}</p></div>)}</div><div className="assistant-suggestions"><button onClick={() => submit('¿Qué productos tienen stock bajo?')}>Stock bajo</button><button onClick={() => submit('¿Cuánto llevamos vendido?')}>Ventas de hoy</button><button onClick={() => submit('¿Cómo exporto el inventario?')}>Exportar</button></div><div className="assistant-input"><input value={question} onChange={(e) => setQuestion(e.target.value)} onKeyDown={(e) => e.key === 'Enter' && submit()} placeholder="Escribe una pregunta..." aria-label="Pregunta al asistente" /><button className="button primary" onClick={() => submit()}>Enviar</button></div><small className="assistant-note">{isSupabaseMode ? 'Modo Supabase PostgreSQL; Watson sigue pendiente de configuración.' : 'Demo local preparada para conectar con Watson Assistant mediante el contrato de integración.'}</small></div></div></div>;
}

function AppRoutes() {
  const authRequired = isSupabaseMode || isSupabaseConfigured;

  return (
    <Routes>
      <Route path="/login" element={<AuthPage mode="login" />} />
      <Route path="/signup" element={<AuthPage mode="signup" />} />
      <Route path="/app/*" element={<ProtectedRoute required={authRequired}><App /></ProtectedRoute>} />
      <Route path="/" element={<Navigate to="/app" replace />} />
      <Route path="*" element={<Navigate to="/app" replace />} />
    </Routes>
  );
}

createRoot(document.getElementById('root')).render(<HashRouter><AuthProvider><AppRoutes /></AuthProvider></HashRouter>);
