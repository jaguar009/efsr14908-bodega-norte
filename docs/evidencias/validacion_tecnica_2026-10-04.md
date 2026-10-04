# Validación técnica — 04-10-2026

Ejecutor: Codex, preparación técnica del proyecto. No representa aceptación firmada de integrantes del Grupo 10 ni mediciones en una bodega real.

- pnpm test: 18/18 aprobadas. Incluye stock, idempotencia, cancelación, archivos históricos, versiones, permisos demo y exportación segura.
- build:github y build:visualstudio: Vite 8.3.1 completó sin advertencia de tamaño. Se separaron aplicación y dependencias en archivos con hash para caché.
- dotnet build: .NET 8, cero errores y cero advertencias.
- javac -encoding UTF-8: completó. El programa Java usa bearer token y todavía no se ejecutó contra el servidor PostgreSQL.
- Supabase: ACTIVE_HEALTHY. Migración bodega_operations_v2 aplicada; 9 tablas, 12 productos y 6 ventas preservados. Cero usuarios Auth al verificar. RLS sin políticas es informativo: esquema privado sin permisos de Data API para clientes.
- Navegador localhost demo: exceso rechazado, limpiar todas las líneas, venta Yape y constancia, cancelación restaura stock, movimientos, proveedores, configuración persistente, cajero sin acciones administrativas y reportes vacíos.
- Se corrigió el cambio de fecha de los controles nativos y el desbordamiento del buscador móvil detectados durante la revisión.
- MCP Supabase adicional: habilitado y autenticación OAuth confirmada. No suministra la conexión SQL al backend.

Despliegue del 04-10-2026: Render publicó bodega-norte-grupo10 con plan gratuito en Juan Diego's workspace. Se guardó la cadena SQL fuera del repositorio y se incluyó el certificado raíz público oficial de Supabase. La conexión exige VerifyFull. /api/health y /api/health/ready respondieron HTTP 200; /api/products y /api/members respondieron 401 sin sesión; preflight CORS respondió 204 y permitió https://jaguar009.github.io. Sin errores de aplicación en Render al consultar. GitHub Pages publicó el frontend con VITE_API_MODE=supabase y la API de Render; los workflows de despliegue y validación terminaron correctamente en el commit 7cce8a0. Supabase Auth permite registro por correo con confirmación y tiene los destinos exactos de Pages y Render.

Pendientes: crear y confirmar la cuenta inicial, validar acceso y operaciones HTTP autenticadas, ejecutar la prueba Java en una base desechable y registrar aceptación del equipo. Los datos de la demostración son ficticios; ninguna de estas pruebas constituye evidencia de asistencia o tareas personales reales.

Revisión final: las nueve páginas cargan; una sesión limpia de navegador no registró errores ni advertencias de consola. Móvil a 390 px (375 px útiles por barra vertical): ancho de documento y viewport 375 px, sin desbordamiento horizontal. Diez archivos Word convertidos con Microsoft Word a PDF y revisados mediante vistas de sus 55 páginas; ningún texto fuera de la página. Se ajustaron párrafos, encabezados de tabla y el modelo de nueve tablas. Word se mantiene en Arial 11 y blanco y negro.
