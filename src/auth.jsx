import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom';
import { isSupabaseConfigured, supabase } from './supabase';
const Context = createContext(null);
const returnUrl = () => new URL(import.meta.env.BASE_URL, window.location.origin).href;
export function AuthProvider({ children }) {
  const [session,setSession] = useState(null), [loading,setLoading] = useState(isSupabaseConfigured);
  useEffect(() => {
    if (!supabase) return;
    let active=true;
    const { data:{subscription} } = supabase.auth.onAuthStateChange((event,next) => {
      if (!active) return;
      setSession(next); setLoading(false);
      if (event === 'PASSWORD_RECOVERY') window.location.hash = '/reset-password';
    });
    supabase.auth.getSession().then(({data,error}) => { if(active) { setSession(error ? null:data.session); setLoading(false); } }).catch(() => { if(active) setLoading(false); });
    return () => { active=false; subscription.unsubscribe(); };
  },[]);
  const value=useMemo(() => ({
    session,user:session?.user ?? null,loading,enabled:isSupabaseConfigured,
    async signIn(email,password) { const {error}=await supabase.auth.signInWithPassword({email,password}); if(error) throw error; },
    async signUp(email,password) { return supabase.auth.signUp({email,password,options:{emailRedirectTo:returnUrl()}}); },
    async signOut() { const {error}=await supabase.auth.signOut(); if(error) throw error; },
  }),[session,loading]);
  return <Context.Provider value={value}>{children}</Context.Provider>;
}
export function useAuth() { return useContext(Context); }
export function ProtectedRoute({children,required}) {
  const {user,loading}=useAuth(), location=useLocation();
  if(!required) return children;
  if(!isSupabaseConfigured) return <AuthSetupPage />;
  if(loading) return <div className="auth-loading" role="status">Comprobando sesión…</div>;
  if(!user) return <Navigate to="/login" replace state={{from:location}} />;
  return children;
}
function AuthFrame({title,description,children}) {
  return <main className="auth-page"><section className="auth-card"><div className="auth-brand"><div className="brand-mark"><span/><span/><span/></div><div><div className="brand-name">Bodega Norte</div><div className="brand-tagline">Inventario y ventas</div></div></div><div className="auth-heading"><h1>{title}</h1><p>{description}</p></div>{children}</section></main>;
}
export function AuthPage({mode='login'}) {
  const {user,loading,enabled,signIn,signUp}=useAuth(), navigate=useNavigate(), location=useLocation();
  const [email,setEmail]=useState(''), [password,setPassword]=useState(''), [busy,setBusy]=useState(false), [error,setError]=useState(''), [notice,setNotice]=useState('');
  const signup=mode==='signup';
  if(!enabled) return <AuthSetupPage/>;
  if(loading) return <div className="auth-loading">Comprobando sesión…</div>;
  if(user) return <Navigate to="/app" replace/>;
  const submit=async event => {
    event.preventDefault(); if(busy) return; setBusy(true); setError(''); setNotice('');
    try {
      if(signup) { const {data,error}=await signUp(email.trim(),password); if(error) throw error; if(!data.session) { setNotice('Revisa tu correo para confirmar la cuenta. Después, el administrador debe habilitar tu acceso.'); return; } }
      else await signIn(email.trim(),password);
      navigate(location.state?.from?.pathname?.startsWith('/app') ? location.state.from.pathname:'/app',{replace:true});
    } catch(e) { setError(e.message); } finally { setBusy(false); }
  };
  return <AuthFrame title={signup ? 'Crear cuenta':'Iniciar sesión'} description={signup ? 'Confirma tu correo. El administrador asignará tu rol antes de que puedas operar.':'Accede con tu cuenta de la bodega.'}><form className="auth-form" onSubmit={submit}><label htmlFor="email">Correo electrónico</label><input id="email" type="email" autoComplete="email" value={email} onChange={e=>setEmail(e.target.value)} required/><label htmlFor="password">Contraseña</label><input id="password" type="password" minLength={signup?8:1} autoComplete={signup ? 'new-password':'current-password'} value={password} onChange={e=>setPassword(e.target.value)} required/>{error&&<p className="auth-message error" role="alert">{error}</p>}{notice&&<p className="auth-message success" role="status">{notice}</p>}<button className="button primary auth-submit" disabled={busy}>{busy?'Un momento…':signup?'Crear cuenta':'Ingresar'}</button></form><p className="auth-switch"><Link to={signup?'/login':'/signup'}>{signup?'Ya tengo cuenta':'Crear una cuenta'}</Link></p><Link className="auth-back" to="/forgot-password">Olvidé mi contraseña</Link></AuthFrame>;
}
export function RecoveryPage({reset=false}) {
  const {user}=useAuth();
  const [value,setValue]=useState(''), [busy,setBusy]=useState(false), [notice,setNotice]=useState(''), [error,setError]=useState('');
  const submit=async e => {
    e.preventDefault(); setBusy(true); setError('');
    try {
      const {error}=reset ? await supabase.auth.updateUser({password:value}) :
        await supabase.auth.resetPasswordForEmail(value.trim(),{redirectTo:returnUrl()+'#/reset-password'});
      if(error) throw error;
      setNotice(reset ? 'Contraseña actualizada. Ya puedes ingresar.':'Si el correo está registrado, recibirás un enlace para cambiar tu contraseña.');
    } catch(e) { setError(e.message); } finally { setBusy(false); }
  };
  if(!supabase) return <AuthSetupPage/>;
  return <AuthFrame title={reset?'Nueva contraseña':'Recuperar acceso'} description={reset?'Abre el enlace del correo para definir tu nueva contraseña.':'Te enviaremos un enlace al correo registrado.'}>{reset&&!user ? <p>Falta una sesión de recuperación. Solicita otro enlace.</p>:<form className="auth-form" onSubmit={submit}><label htmlFor="recovery">{reset?'Nueva contraseña':'Correo electrónico'}</label><input id="recovery" type={reset?'password':'email'} autoComplete={reset?'new-password':'email'} minLength={reset?8:undefined} value={value} onChange={e=>setValue(e.target.value)} required/><button className="button primary auth-submit" disabled={busy}>{busy?'Un momento…':reset?'Actualizar contraseña':'Enviar enlace'}</button></form>}{error&&<p className="auth-message error" role="alert">{error}</p>}{notice&&<p className="auth-message success" role="status">{notice}</p>}<Link className="auth-back" to="/login">Volver al inicio de sesión</Link></AuthFrame>;
}
export function AuthSetupPage() { return <AuthFrame title="Configuración incompleta" description="La conexión de autenticación todavía no está configurada. Contacta al administrador."/>; }
