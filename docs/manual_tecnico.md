# Manual técnico — Bodega Norte

## Arquitectura

ASP.NET Core 8 MVC: React es la View; Controllers reciben las solicitudes; Contracts y BodegaService definen los datos y reglas; BodegaRepository accede a PostgreSQL mediante Npgsql. HomeController resuelve los archivos con hash del manifiesto de Vite. GitHub Pages sirve la interfaz estática y necesita el backend externo para compartir datos.

La demostración usa `src/demoApi.js` y `bodega-norte:v2:data` en localStorage. Migra registros antiguos sin inventar fechas o costos. Es un entorno local con datos ficticios; no certifica una operación remota.

## Autenticación y permisos

El navegador usa la URL y clave publishable de Supabase. Envía el token de sesión en Authorization: Bearer. El servidor valida la sesión con Supabase Auth y consulta members en cada solicitud. No confía en roles de user_metadata. BootstrapAdminEmail solo habilita automáticamente un correo confirmado coincidente. Los demás usuarios requieren habilitación del administrador.

| Operación | Cajero | Administrador |
|---|---|---|
| Consultas de catálogo, ventas, configuración y movimientos | Sí | Sí |
| Registrar venta | Sí | Sí |
| CRUD catálogo/proveedores, ajustes, anulación | No | Sí |
| Configuración, roles, auditoría, respaldo | No | Sí |

El esquema privado bodega_norte contiene categories, suppliers, products, sales, sale_items, members, settings, stock_movements y audit_log. RLS está activado y se revocaron permisos de anon/authenticated. El acceso SQL del backend requiere una credencial privada; el cliente no consulta estas tablas mediante Data API. La advertencia informativa RLS sin políticas corresponde a esta decisión de acceso exclusivo por servidor.

## Consistencia

- Códigos UUID; crear y editar son operaciones separadas.
- Edición y ajuste requieren expectedVersion. El ajuste exige motivo y rechaza saldo negativo.
- POST /api/sales requiere requestId UUID y líneas con productId, quantity y expectedPrice. El servidor calcula importes con su precio y verifica el precio visto por el cajero.
- Los productos se bloquean en orden estable dentro de una transacción. Stock, venta, detalles, movimientos y auditoría se confirman juntos.
- Reintentar requestId con el mismo contenido y actor devuelve la misma venta. Cambiar el contenido de esa clave devuelve conflicto.
- Anular devuelve existencias una vez y conserva el registro. Archivar productos no borra sus ventas.
- Nombre, costo y precio de las líneas se capturan al vender. Los costos antiguos desconocidos permanecen nulos.
- Reportes usan America/Lima, ventas activas y rangos de fechas reales. No rellenan días vacíos con cifras de ejemplo.

## Endpoints

| Ruta | Método y uso |
|---|---|
| /api/me | GET perfil y rol |
| /api/products | GET catálogo, POST crear |
| /api/products/{id} | PUT editar, DELETE archivar |
| /api/products/{id}/stock | POST ajustar con motivo |
| /api/sales | GET historial, POST venta idempotente |
| /api/sales/{id}/cancel | POST anular |
| /api/suppliers | GET y POST |
| /api/suppliers/{id} | PUT y DELETE |
| /api/settings | GET y PUT |
| /api/members | GET y PUT |
| /api/movements, /api/audit, /api/backup | GET |
| /api/health | GET estado del proceso sin información privada |
| /api/health/ready | GET comprueba disponibilidad de la base |

## Respaldo y recuperación

Configuración permite descargar JSON versión 2 con las nueve tablas de negocio. No contiene contraseñas ni sesiones de Auth. Una transacción repeatable read produce una copia consistente. Auditoría registra su exportación. No se ofrece restauración sobre la base activa desde el navegador.

Para recuperar: crear una base aislada; aplicar schema.sql y upgrade_operations.sql; conservar el JSON original; importar tablas en orden categories, suppliers, products, sales, sale_items, settings, members, stock_movements, audit_log con sus claves; verificar cuentas Auth antes de recuperar members; ajustar secuencias; comparar conteos, totales y claves foráneas. La importación JSON requiere un procedimiento técnico específico y no se ha validado aún. Para una recuperación completa del servidor se necesita un respaldo PostgreSQL independiente, incluido Auth cuando corresponda. No se anuncian copias automáticas ni recuperación probada.

## Publicación y límites

Docker compila React y MVC para Render. PORT configura el puerto; ConnectionStrings__BodegaNorte se guarda como secreto; BootstrapAdminEmail identifica la cuenta inicial. CORS permite el origen de GitHub Pages. Supabase Auth requiere redirect URLs con la ruta completa del repositorio y recuperación de contraseña.

El backend está publicado en https://bodega-norte-grupo10.onrender.com/ y Pages usa la API remota. Se verificaron HTTP 200 en /api/health y /api/health/ready, HTTP 401 sin sesión en rutas privadas y preflight CORS 204 para Pages. La conexión usa VerifyFull y el certificado raíz oficial público de Supabase, incluido y copiado a la publicación. Falta confirmar la cuenta inicial y validar las operaciones autenticadas. Las 18 pruebas locales y builds no sustituyen esa prueba. La prueba Java compilada requiere BODEGA_API_TOKEN y una base desechable. Watson, compras, impuestos, pagos bancarios y facturación electrónica no están integrados.
