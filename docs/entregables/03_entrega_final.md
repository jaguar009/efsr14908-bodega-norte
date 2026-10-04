# Informe final del proyecto

El informe editable solicitado se encuentra en `docs/word/03_Entrega_Final_Informe.docx`. Bodega Norte es un prototipo académico con datos ficticios; el proyecto no se desplegó ni se validó en una bodega real.

## Componentes

- Frontend React y Vite, con modo demo en `localStorage`.
- Backend ASP.NET Core 8 MVC con controladores, servicio y repositorio Npgsql para Supabase PostgreSQL.
- Esquema y semilla de demostración en el esquema privado `bodega_norte`.
- Diagramas de arquitectura y modelo de datos, en formatos editables y blanco y negro.
- Backlog, cronograma, plan de integración continua y matriz de pruebas.
- Informe, manuales, guía de instalación, guion de sustentación y especificación Watson en Word.

## Verificación

El proyecto Supabase **Bodega Norte Grupo 10** (`pladdberkeewsvomkset`, región `sa-east-1`) de la nueva cuenta ya tiene el esquema y la semilla de demostración. La consulta remota confirmó 7 categorías, 5 proveedores, 12 productos, 6 ventas y 15 detalles. `.env.visualstudio` ya apunta a este proyecto. La conexión Npgsql de la API desde Visual Studio y la prueba HTTP todavía requieren configurar la contraseña de PostgreSQL en User Secrets; no se declara una prueba de venta sobre la base remota.

## Límites

El prototipo implementa autenticación, roles y auditoría. Facturación electrónica y compras están fuera del alcance; el despliegue compartido sigue pendiente. Watson Assistant está documentado como integración futura y no está conectado. La fecha de entrega final debe confirmarse debido a diferencias entre el sílabo 2026 y el Anexo 4 Nivel 2.

## Actualización al 04-10-2026

La versión incorpora roles administrador/cajero, ajustes con motivo, venta idempotente, anulación, detalle y constancia interna, proveedores/configuración persistentes, auditoría y respaldo JSON. Pasaron 18 pruebas locales; la integración HTTP PostgreSQL y el despliegue compartido siguen pendientes. Consultar docs/evidencias/validacion_tecnica_2026-10-04.md. No se certifican asistencia, horas o aceptación de negocio.
