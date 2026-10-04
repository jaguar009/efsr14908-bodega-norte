-- Ampliación aditiva. No elimina ni reemplaza ventas o productos existentes.
ALTER TABLE bodega_norte.products ADD COLUMN IF NOT EXISTS version BIGINT NOT NULL DEFAULT 1;
ALTER TABLE bodega_norte.sales ADD COLUMN IF NOT EXISTS request_id UUID UNIQUE;
ALTER TABLE bodega_norte.sales ADD COLUMN IF NOT EXISTS request_hash TEXT;
ALTER TABLE bodega_norte.sales ADD COLUMN IF NOT EXISTS created_by UUID;
ALTER TABLE bodega_norte.sales ADD COLUMN IF NOT EXISTS actor_email TEXT NOT NULL DEFAULT 'Datos de demostración';
ALTER TABLE bodega_norte.sales ADD COLUMN IF NOT EXISTS customer VARCHAR(120) NOT NULL DEFAULT 'Venta mostrador';
ALTER TABLE bodega_norte.sales ADD COLUMN IF NOT EXISTS status TEXT NOT NULL DEFAULT 'active' CHECK (status IN ('active','cancelled'));
ALTER TABLE bodega_norte.sales ADD COLUMN IF NOT EXISTS cancelled_at TIMESTAMPTZ;
ALTER TABLE bodega_norte.sales ADD COLUMN IF NOT EXISTS cancelled_by UUID;
ALTER TABLE bodega_norte.sales ADD COLUMN IF NOT EXISTS cancel_reason VARCHAR(300);
ALTER TABLE bodega_norte.sale_items ADD COLUMN IF NOT EXISTS product_name TEXT;
ALTER TABLE bodega_norte.sale_items ADD COLUMN IF NOT EXISTS unit_cost NUMERIC(12,2);
-- No se inventa el costo histórico de las ventas previas.
CREATE TABLE IF NOT EXISTS bodega_norte.members (
  user_id UUID PRIMARY KEY REFERENCES auth.users(id), email TEXT NOT NULL UNIQUE,
  role TEXT NOT NULL CHECK (role IN ('admin','cashier')), is_active BOOLEAN NOT NULL DEFAULT TRUE
);
CREATE TABLE IF NOT EXISTS bodega_norte.settings (
  id INTEGER PRIMARY KEY CHECK (id = 1), name VARCHAR(120) NOT NULL, ruc VARCHAR(11) NOT NULL DEFAULT '',
  address VARCHAR(200) NOT NULL DEFAULT '', phone VARCHAR(30) NOT NULL DEFAULT '', stock_alerts BOOLEAN NOT NULL DEFAULT TRUE
);
INSERT INTO bodega_norte.settings(id,name) VALUES (1,'Bodega Norte') ON CONFLICT (id) DO NOTHING;
CREATE TABLE IF NOT EXISTS bodega_norte.stock_movements (
  movement_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  product_id INTEGER NOT NULL REFERENCES bodega_norte.products(product_id),
  quantity NUMERIC(12,2) NOT NULL, balance NUMERIC(12,2) NOT NULL CHECK (balance >= 0),
  type TEXT NOT NULL CHECK (type IN ('initial','sale','adjustment','cancellation')),
  reason VARCHAR(300) NOT NULL, actor_id UUID, actor_email TEXT NOT NULL, created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE TABLE IF NOT EXISTS bodega_norte.audit_log (
  audit_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, action TEXT NOT NULL, entity TEXT NOT NULL, entity_id TEXT NOT NULL,
  actor_id UUID, actor_email TEXT NOT NULL, details JSONB NOT NULL DEFAULT '{}', created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_movements_product_date ON bodega_norte.stock_movements(product_id, created_at DESC);
CREATE INDEX IF NOT EXISTS ix_audit_created ON bodega_norte.audit_log(created_at DESC);
ALTER TABLE bodega_norte.members ENABLE ROW LEVEL SECURITY;
ALTER TABLE bodega_norte.settings ENABLE ROW LEVEL SECURITY;
ALTER TABLE bodega_norte.stock_movements ENABLE ROW LEVEL SECURITY;
ALTER TABLE bodega_norte.audit_log ENABLE ROW LEVEL SECURITY;
REVOKE ALL ON ALL TABLES IN SCHEMA bodega_norte FROM anon, authenticated;
REVOKE ALL ON ALL SEQUENCES IN SCHEMA bodega_norte FROM anon, authenticated;
