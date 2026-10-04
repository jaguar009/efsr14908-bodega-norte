# Supabase PostgreSQL

El proyecto Supabase **Bodega Norte Grupo 10** (`pladdberkeewsvomkset`, región `sa-east-1`) de la nueva cuenta ya contiene el esquema privado `bodega_norte` y los datos ficticios. La verificación remota encontró 7 categorías, 5 proveedores, 12 productos, 6 ventas y 15 detalles de venta. Para reconstruirlo en otro proyecto, ejecuta `schema.sql`, `upgrade_operations.sql` y `seed.sql`, en ese orden en el SQL Editor.

Guarda la cadena de conexión Npgsql en **Manage User Secrets** del proyecto `backend/BodegaNorte.Api`, bajo `ConnectionStrings:BodegaNorte`. Usa los datos que muestra **Connect → Session pooler** en Supabase:

```json
{
  "ConnectionStrings": {
    "BodegaNorte": "Host=HOST_DEL_SESSION_POOLER;Port=5432;Database=postgres;Username=USUARIO_DEL_POOLER;Password=TU_CONTRASEÑA;SSL Mode=Require"
  }
}
```

Reemplaza los campos del host, usuario y contraseña con los valores del proyecto. Mantén SSL activo. No publiques la contraseña de PostgreSQL ni la guardes en `appsettings.json`; la interfaz React llama a la API MVC y solo utiliza la clave publishable. La contraseña PostgreSQL permanece en el servidor.

Las tablas viven fuera del esquema público y tienen RLS habilitado. El backend implementa Supabase Auth y permisos administrador/cajero consultados en members. Los clientes anon/authenticated no tienen acceso directo a estas tablas. La conexión privada y la integración HTTP remota siguen pendientes de verificar. La migración bodega_operations_v2 ya se aplicó sin eliminar datos.
