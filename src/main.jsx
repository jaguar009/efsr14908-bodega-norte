import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { createRoot } from 'react-dom/client';
import { HashRouter, Navigate, Route, Routes, useLocation, useNavigate } from 'react-router-dom';
import './styles.css';
import { inventoryApi as api, isSupabaseMode } from './api';
import { AuthPage, AuthProvider, ProtectedRoute, RecoveryPage, useAuth } from './auth';
import { Icon } from './Icon';
import { round, uid, updateCart, validateSettings } from './domain';
import { Assistant, Button, Checkout, Dashboard, History, Inventory, LowStock, Modal, Movements, ProductForm, ReasonForm, Receipt, Reports, Sales, Settings, SupplierForm, Suppliers, download } from './ui';
const navigation=[
  ['dashboard','Inicio','home'],['sales','Nueva venta','cart'],['history','Historial de ventas','receipt'],
  ['inventory','Inventario','box'],['products','Productos','tag'],['suppliers','Proveedores','users'],
  ['movements','Movimientos de stock','box'],['reports','Reportes','chart'],['settings','Configuración','settings'],
];
const empty={products:[],sales:[],suppliers:[],movements:[],members:[],audit:[],settings:{name:'Bodega Norte',ruc:'',address:'',phone:'',stockAlerts:true}};
function stored(key) { try{return JSON.parse(localStorage.getItem(key));}catch{return null;} }
function App() {
  const location=useLocation(),navigate=useNavigate(),{user,signOut}=useAuth();
  const page=location.pathname.split('/')[2]||'dashboard';
  const pendingKey='bodega-norte:pending-sale:'+(user?.id||'demo');
  const initialPending=useMemo(()=>stored(pendingKey),[pendingKey]);
  const [data,setData]=useState(empty),[profile,setProfile]=useState(null),[loading,setLoading]=useState(true),[loadError,setLoadError]=useState('');
  const [cart,setCart]=useState(initialPending?.cart||[]),[pending,setPending]=useState(initialPending);
  const [query,setQuery]=useState(''),[modal,setModal]=useState(null),[busy,setBusy]=useState(false),[toast,setToast]=useState(null),[mobileNav,setMobileNav]=useState(false);
  const themeKey='bodega-norte:theme:'+(user?.id||'demo');
  const [dark,setDark]=useState(()=>localStorage.getItem(themeKey)==='dark');
  const lock=useRef(false),toastTimer=useRef(null),generation=useRef(0);
  const canManage=profile?.role==='admin';
  const lowStock=useMemo(()=>data.products.filter(p=>p.stock<=p.minStock),[data.products]);
  const notify=useCallback((message,error=false)=>{clearTimeout(toastTimer.current);setToast({message,error});toastTimer.current=setTimeout(()=>setToast(null),error?8000:4000);},[]);
  useEffect(()=>()=>clearTimeout(toastTimer.current),[]);
  useEffect(()=>{localStorage.setItem(themeKey,dark?'dark':'light');},[dark,themeKey]);
  const clearPending=useCallback(()=>{localStorage.removeItem(pendingKey);setPending(null);},[pendingKey]);
  const load=useCallback(async()=>{
    const version=++generation.current;
    try {
      const person=await api.getProfile();
      if(version!==generation.current)return;
      setProfile(person);
      if(person.role!=='admin'&&person.role!=='cashier'){setLoading(false);setLoadError('');return;}
      const [products,sales,suppliers,settings,movements,members,audit]=await Promise.all([
        api.getProducts(),api.getSales(),api.getSuppliers(),api.getSettings(),api.getMovements(),
        person.role==='admin'?api.getMembers():Promise.resolve([]),person.role==='admin'?api.getAudit():Promise.resolve([]),
      ]);
      if(version!==generation.current)return;
      setData({products,sales,suppliers,settings,movements,members,audit});setLoadError('');
      const unresolved=stored(pendingKey);
      if(unresolved){const sale=sales.find(s=>s.requestId===unresolved.payload.requestId);if(sale){clearPending();setCart([]);setModal({type:'receipt',sale});notify('Se confirmó el cobro pendiente sin duplicarlo.');}}
      else setCart(current=>current.map(line=>{const p=products.find(p=>p.id===line.id);return p?{...p,quantity:line.quantity}:null;}).filter(Boolean));
    } catch(error) { if(version===generation.current)setLoadError(error.message); }
    finally { if(version===generation.current)setLoading(false); }
  },[pendingKey,clearPending,notify]);
  useEffect(()=>{
    load();const interval=setInterval(load,30000);
    const refresh=()=>load();window.addEventListener('online',refresh);
    if(!isSupabaseMode)window.addEventListener('storage',refresh);
    return()=>{generation.current++;clearInterval(interval);window.removeEventListener('online',refresh);window.removeEventListener('storage',refresh);};
  },[load]);
  const go=target=>{navigate(target==='dashboard'?'/app':'/app/'+target);setMobileNav(false);setQuery('');};
  const run=async(action,message,keepModal=false)=>{
    if(lock.current)return;lock.current=true;setBusy(true);
    try {const result=await action();if(!keepModal)setModal(null);notify(message);await load();return result;}
    catch(error){notify(error.message,true);throw error;}
    finally{lock.current=false;setBusy(false);}
  };
  const safe=(action,message)=>run(action,message).catch(()=>{});
  // La validación se hace antes del updater para mostrar el error sin efectos dentro del render.
  const add=p=>{
    if(busy||pending)return;
    const next=(cart.find(l=>l.id===p.id)?.quantity||0)+1;
    try{const changed=updateCart(cart,data.products,p.id,next);setCart(changed);}catch(e){notify(e.message,true);}
  };
  const setQuantity=(id,quantity)=>{
    if(busy||pending)return;
    try{updateCart(cart,data.products,id,quantity);setCart(current=>updateCart(current,data.products,id,quantity));}catch(e){notify(e.message,true);}
  };
  const completeSale=async(method,customer)=>{
    if(lock.current||!cart.length)return;lock.current=true;setBusy(true);
    const request=pending||{payload:{paymentMethod:method,customer,requestId:uid(),items:cart.map(l=>({productId:l.id,quantity:l.quantity,expectedPrice:l.price}))},cart};
    try{
      localStorage.setItem(pendingKey,JSON.stringify(request));setPending(request);
      const sale=await api.createSale(request.payload);
      clearPending();setCart([]);setModal({type:'receipt',sale});notify('Venta registrada: '+sale.id.slice(0,18));await load();
    }catch(error){
      // 4xx confirma que la operación fue rechazada. Un fallo de red conserva la clave.
      if(!isSupabaseMode||[400,401,403,404,409,422].includes(error.status))clearPending();
      notify(error.message,true);await load();
    }finally{lock.current=false;setBusy(false);}
  };
  if(loading)return <main className="auth-loading" role="status">Cargando la bodega…</main>;
  if(profile?.role==='pending')return <main className="auth-page"><section className="auth-card"><h1>Acceso pendiente</h1><p>Tu cuenta está creada. El administrador debe habilitarte en Usuarios y permisos.</p><p>{profile.email}</p><Button onClick={load}>Comprobar acceso</Button><button className="auth-back" onClick={()=>signOut().then(()=>navigate('/login'))}>Cerrar sesión</button></section></main>;
  const title=modal?.type==='product'?(modal.product?'Editar producto':'Agregar producto'):modal?.type==='supplier'?(modal.supplier?'Editar proveedor':'Nuevo proveedor'):modal?.type==='stock'?'Ajustar stock':modal?.type==='cancel'?'Anular venta':modal?.type==='checkout'?'Confirmar cobro':modal?.type==='receipt'?'Detalle de venta':modal?.type==='delete'?'Confirmar eliminación':modal?.type==='notifications'?'Productos por reponer':'Asistente de bodega';
  const manageable=canManage&&!busy&&!loadError;
  return <><div className={'app '+(dark?'dark':'')} inert={modal?true:undefined}><aside className={'sidebar '+(mobileNav?'mobile-open':'')}><div className="brand"><div className="brand-mark"><span/><span/><span/></div><div><div className="brand-name">{data.settings.name}</div><div className="brand-tagline">Inventario y ventas</div></div></div><nav className="nav" aria-label="Navegación principal">{navigation.map(([id,label,icon])=><button key={id} className={'nav-item '+(page===id?'active':'')} onClick={()=>go(id)}><Icon name={icon}/><span>{label}</span>{id==='inventory'&&data.settings.stockAlerts&&lowStock.length>0&&<span className="nav-count">{lowStock.length}</span>}</button>)}</nav><div className="sidebar-footer"><span className="status-dot"/><span>{isSupabaseMode?'Base compartida Supabase':'Demostración local'}</span></div></aside><main className="main"><header className="topbar"><button className="mobile-menu" aria-label="Abrir menú" onClick={()=>setMobileNav(!mobileNav)}><Icon name="menu"/></button><div className="search-shell"><Icon name="search"/><input aria-label="Buscar productos" value={query} onChange={e=>setQuery(e.target.value)} placeholder="Buscar productos o proveedores…"/></div><div className="topbar-actions"><button aria-label="Asistente" className="assistant-button" onClick={()=>setModal({type:'assistant'})}><Icon name="help" size={18}/><span>Asistente</span></button><button className="icon-button" aria-label="Ver alertas de stock" onClick={()=>setModal({type:'notifications'})}><Icon name="bell"/>{data.settings.stockAlerts&&lowStock.length>0&&<span className="notification-dot"/>}</button><span className="profile-copy"><strong>{profile?.email?.split('@')[0]||'Usuario'}</strong><small>{canManage?'Administrador':'Cajero'}</small></span>{isSupabaseMode?<button className="logout-button" onClick={()=>signOut().then(()=>navigate('/login')).catch(e=>notify(e.message,true))}>Salir</button>:<select className="select demo-role" aria-label="Simular rol" value={profile?.role||'admin'} onChange={e=>{localStorage.setItem('bodega-norte:demo-role',e.target.value);load();}}><option value="admin">Demo admin</option><option value="cashier">Demo cajero</option></select>}</div></header><div className="content">{!isSupabaseMode&&<p className="notice">Demostración: datos ficticios guardados en este navegador. Los roles simulan los permisos del servidor.</p>}{loadError&&<div className="error-banner" role="alert"><strong>No se pudo actualizar la bodega.</strong> {loadError} <button className="text-button" onClick={load}>Reintentar conexión</button></div>}{pending&&<div className="notice">Hay un cobro pendiente de confirmación. <button className="text-button" onClick={()=>{go('sales');setModal({type:'checkout'});}}>Reintentar el mismo cobro</button></div>}<div className="sync-controls"><span>{busy?'Guardando operación…':'Información compartida: actualización cada 30 segundos'}</span><button className="text-button" disabled={busy} onClick={load}>Actualizar</button></div>{page==='dashboard'&&<Dashboard data={data} lowStock={lowStock} name={profile?.email?.split('@')[0]||'Usuario'} canManage={manageable} onNavigate={go} onAdd={()=>setModal({type:'product'})}/>} {page==='sales'&&<Sales products={data.products} cart={cart} query={query} setQuery={setQuery} onAdd={add} onUpdate={setQuantity} onClear={()=>setCart([])} onCheckout={()=>setModal({type:'checkout'})} locked={busy||Boolean(pending)||Boolean(loadError)}/>} {['inventory','products'].includes(page)&&<Inventory catalog={page==='products'} products={data.products} query={query} setQuery={setQuery} canManage={manageable} onAdd={()=>setModal({type:'product'})} onEdit={product=>setModal({type:'product',product})} onStock={product=>setModal({type:'stock',product})} onDelete={product=>setModal({type:'delete',product})}/>} {page==='suppliers'&&<Suppliers suppliers={data.suppliers} canManage={manageable} onAdd={()=>setModal({type:'supplier'})} onEdit={supplier=>setModal({type:'supplier',supplier})} onDelete={supplier=>setModal({type:'delete',supplier})} onView={name=>{go('inventory');setQuery(name);}}/>} {page==='history'&&<History sales={data.sales} canManage={manageable} onDetail={sale=>setModal({type:'receipt',sale})} onCancel={sale=>setModal({type:'cancel',sale})}/>} {page==='movements'&&<Movements movements={data.movements}/>} {page==='reports'&&<Reports data={data} lowStock={lowStock}/>} {page==='settings'&&<Settings settings={data.settings} members={data.members} audit={data.audit} canManage={canManage} profile={profile||{}} dark={dark} setDark={setDark} busy={busy||Boolean(loadError)} onSave={async settings=>{validateSettings(settings);return run(()=>api.saveSettings(settings),'Configuración guardada.',true);}} onMember={input=>run(()=>api.saveMember(input),'Permisos actualizados.',true)} onBackup={()=>safe(async()=>download('bodega-norte-respaldo-'+new Date().toISOString().slice(0,10)+'.json',JSON.stringify(await api.backup(),null,2),'application/json'),'Respaldo descargado.')}/>}</div></main></div>{modal&&<div className={dark?'app dark modal-theme':'app modal-theme'}><Modal title={title} onClose={()=>!busy&&setModal(null)} busy={busy} receipt={modal.type==='receipt'}>{modal.type==='product'&&<ProductForm product={modal.product} suppliers={data.suppliers} busy={busy} onSave={(input,id)=>safe(()=>api.saveProduct(input,id),'Producto guardado.')}/>} {modal.type==='supplier'&&<SupplierForm supplier={modal.supplier} busy={busy} onSave={(input,id)=>safe(()=>api.saveSupplier(input,id),'Proveedor guardado.')}/>} {modal.type==='stock'&&<ReasonForm stock product={modal.product} busy={busy} description={modal.product.name+' · Stock actual: '+modal.product.stock} onSave={input=>safe(()=>api.adjustStock(modal.product.id,input),'Ajuste registrado.')}/>} {modal.type==='cancel'&&<ReasonForm busy={busy} description={'Se devolverán al inventario las unidades de la venta '+modal.sale.id+'.'} onSave={reason=>safe(()=>api.cancelSale(modal.sale.id,reason),'Venta anulada y stock restaurado.')}/>} {modal.type==='checkout'&&<Checkout cart={cart} busy={busy} pending={pending} onConfirm={completeSale}/>} {modal.type==='receipt'&&<Receipt sale={modal.sale} settings={data.settings}/>} {modal.type==='notifications'&&<LowStock products={data.settings.stockAlerts?lowStock:[]}/>} {modal.type==='assistant'&&<Assistant data={data} lowStock={lowStock}/>} {modal.type==='delete'&&<><div className="modal-form"><p>¿Eliminar {modal.product?.name||modal.supplier?.name} del catálogo activo?</p><p>Se conservará el historial registrado.</p></div><div className="modal-footer"><Button disabled={busy} onClick={()=>safe(()=>modal.product?api.deleteProduct(modal.product.id):api.deleteSupplier(modal.supplier.id),'Registro eliminado.')}>Confirmar eliminación</Button></div></>}</Modal></div>}{toast&&<div className={'toast '+(toast.error?'error':'')} role={toast.error?'alert':'status'}>{toast.message}</div>}</>;
}
function AppRoutes() {
  return <Routes><Route path="/login" element={<AuthPage/>}/><Route path="/signup" element={<AuthPage mode="signup"/>}/><Route path="/forgot-password" element={<RecoveryPage/>}/><Route path="/reset-password" element={<RecoveryPage reset/>}/><Route path="/app/*" element={<ProtectedRoute required={isSupabaseMode}><App/></ProtectedRoute>}/><Route path="*" element={<Navigate to="/app" replace/>}/></Routes>;
}
class ErrorBoundary extends React.Component {
  state={failed:false};
  static getDerivedStateFromError(){return {failed:true};}
  render(){return this.state.failed?<main className="auth-page"><section className="auth-card"><h1>No se pudo mostrar la página</h1><p>Recarga la aplicación para continuar. Un cobro pendiente conservará su identificador.</p><Button onClick={()=>window.location.reload()}>Recargar</Button></section></main>:this.props.children;}
}
const appRoot=import.meta.hot?.data.root??createRoot(document.getElementById('root'));
if(import.meta.hot)import.meta.hot.data.root=appRoot;
appRoot.render(<ErrorBoundary><HashRouter><AuthProvider><AppRoutes/></AuthProvider></HashRouter></ErrorBoundary>);
