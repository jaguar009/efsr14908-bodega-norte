import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import { Link, Navigate, useLocation } from 'react-router-dom';
import { isSupabaseConfigured, supabase } from './supabase';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [session, setSession] = useState(null);
  const [loading, setLoading] = useState(isSupabaseConfigured);

  useEffect(() => {
    if (!supabase) return undefined;

    let active = true;
    const { data: { subscription } } = supabase.auth.onAuthStateChange((_event, nextSession) => {
      if (!active) return;
      setSession(nextSession);
      setLoading(false);
    });

    supabase.auth.getSession().then(({ data, error }) => {
      if (!active) return;
      setSession(error ? null : data.session);
      setLoading(false);
    }).catch(() => {
      if (!active) return;
      setSession(null);
      setLoading(false);
    });

    return () => {
      active = false;
      subscription.unsubscribe();
    };
  }, []);

  const value = useMemo(() => ({
    session,
    user: session?.user ?? null,
    loading,
    enabled: isSupabaseConfigured,
    async signIn(email, password) {
      if (!supabase) throw new Error('Falta configurar Supabase Auth.');
      const { error } = await supabase.auth.signInWithPassword({ email, password });
      if (error) throw error;
    },
    async signUp(email, password) {
      if (!supabase) throw new Error('Falta configurar Supabase Auth.');
      return supabase.auth.signUp({
        email,
        password,
        options: { emailRedirectTo: window.location.origin },
      });
    },
    async signOut() {
      if (!supabase) return;
      const { error } = await supabase.auth.signOut();
      if (error) throw error;
    },
  }), [session, loading]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth debe usarse dentro de AuthProvider.');
  return context;
}

export function ProtectedRoute({ children, required }) {
  const { loading, user } = useAuth();
  const location = useLocation();

  if (!required) return children;
  if (!isSupabaseConfigured) return <AuthSetupPage />;
  if (loading) return <div className="auth-loading" role="status">Comprobando sesión…</div>;
  if (!user) return <Navigate to="/login" replace state={{ from: location }} />;
  return children;
}

export function AuthPage({ mode = 'login' }) {
  const { enabled, loading, user, signIn, signUp } = useAuth();
  const location = useLocation();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const isSignUp = mode === 'signup';

  if (!enabled) return <AuthSetupPage />;
  if (loading) return <div className="auth-loading" role="status">Comprobando sesión…</div>;
  if (user) return <Navigate to="/app" replace />;

  const submit = async (event) => {
    event.preventDefault();
    setSubmitting(true);
    setError('');
    setNotice('');

    try {
      if (isSignUp) {
        const { data, error: signUpError } = await signUp(email.trim(), password);
        if (signUpError) throw signUpError;
        if (!data.session) {
          setNotice('Revisa tu correo y confirma la cuenta para continuar.');
          return;
        }
      } else {
        await signIn(email.trim(), password);
      }

      const from = location.state?.from;
      const destination = from?.pathname?.startsWith('/app') ? `${from.pathname}${from.search || ''}` : '/app';
      window.location.hash = destination;
    } catch (authError) {
      setError(authError.message || 'No se pudo completar la autenticación.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <main className="auth-page">
      <section className="auth-card" aria-labelledby="auth-title">
        <div className="auth-brand">
          <div className="brand-mark"><span></span><span></span><span></span></div>
          <div><div className="brand-name">Bodega Norte</div><div className="brand-tagline">Tu barrio, siempre contigo</div></div>
        </div>
        <div className="auth-heading">
          <div className="page-eyebrow">Acceso seguro</div>
          <h1 id="auth-title">{isSignUp ? 'Crear cuenta' : 'Bienvenido de nuevo'}</h1>
          <p>{isSignUp ? 'Crea tu acceso para administrar la bodega.' : 'Inicia sesión para continuar a tu bodega.'}</p>
        </div>
        <form className="auth-form" onSubmit={submit}>
          <label htmlFor="auth-email">Correo electrónico</label>
          <input id="auth-email" type="email" autoComplete="email" value={email} onChange={(event) => setEmail(event.target.value)} placeholder="tu@correo.com" required />
          <label htmlFor="auth-password">Contraseña</label>
          <input id="auth-password" type="password" autoComplete={isSignUp ? 'new-password' : 'current-password'} minLength={6} value={password} onChange={(event) => setPassword(event.target.value)} placeholder="Mínimo 6 caracteres" required />
          {error && <p className="auth-message error" role="alert">{error}</p>}
          {notice && <p className="auth-message success" role="status">{notice}</p>}
          <button className="button primary auth-submit" type="submit" disabled={submitting}>
            {submitting ? 'Un momento…' : isSignUp ? 'Crear cuenta' : 'Iniciar sesión'}
          </button>
        </form>
        <p className="auth-switch">
          {isSignUp ? '¿Ya tienes una cuenta?' : '¿Aún no tienes una cuenta?'}{' '}
          <Link to={isSignUp ? '/login' : '/signup'}>{isSignUp ? 'Inicia sesión' : 'Crear cuenta'}</Link>
        </p>
        <Link className="auth-back" to="/app">Volver a Bodega Norte</Link>
      </section>
    </main>
  );
}

export function AuthSetupPage() {
  return (
    <main className="auth-page">
      <section className="auth-card auth-setup" aria-labelledby="auth-setup-title">
        <div className="auth-brand">
          <div className="brand-mark"><span></span><span></span><span></span></div>
          <div><div className="brand-name">Bodega Norte</div><div className="brand-tagline">Tu barrio, siempre contigo</div></div>
        </div>
        <div className="auth-heading">
          <div className="page-eyebrow">Falta un paso</div>
          <h1 id="auth-setup-title">Configura Supabase Auth</h1>
          <p>Agrega la URL del proyecto y su clave publishable para habilitar el inicio de sesión.</p>
        </div>
        <div className="auth-config-list">
          <code>VITE_SUPABASE_URL</code>
          <code>VITE_SUPABASE_PUBLISHABLE_KEY</code>
          <span>En Visual Studio, guarda también <code>Supabase:Url</code> y <code>Supabase:PublishableKey</code> en User Secrets.</span>
        </div>
      </section>
    </main>
  );
}
