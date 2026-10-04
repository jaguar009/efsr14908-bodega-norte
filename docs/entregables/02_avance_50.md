# Informe de avance del proyecto

El documento editable de avance está en `docs/word/02_Entregable_2_Avance_50.docx`. Resume el estado del código al 28 de septiembre de 2026; no certifica el porcentaje que el Grupo 10 presentó en una semana específica.

## Alcance comprobado

- Interfaz React para inicio, productos, ventas, inventario, reportes y configuración.
- API ASP.NET Core con catálogo de productos y registro transaccional de ventas.
- Backend MVC con Npgsql y scripts Supabase PostgreSQL para datos de demostración.
- Compilación React para Visual Studio y backend ASP.NET Core 8 sin errores el 28 de septiembre de 2026.
- El proyecto Supabase de desarrollo ya contiene el esquema y los datos de demostración; faltan la cadena con contraseña en User Secrets y la prueba HTTP de venta/stock desde la API local.

## Pendiente del equipo

- Aceptación manual de interfaz, responsive y exportación CSV.
- Registrar ejecutor, fecha y capturas reales de aceptación.
- Confirmar con la docente qué semana y fecha aplican al hito final.
- Sustituir datos de demostración solo con autorización del negocio, si se valida el caso.
- Configurar la contraseña de Supabase en User Secrets y verificar el API solo contra el proyecto de desarrollo.

## Actualización al 04-10-2026

La versión incorpora roles administrador/cajero, ajustes con motivo, venta idempotente, anulación, detalle y constancia interna, proveedores/configuración persistentes, auditoría y respaldo JSON. Pasaron 18 pruebas locales; la integración HTTP PostgreSQL y el despliegue compartido siguen pendientes. Consultar docs/evidencias/validacion_tecnica_2026-10-04.md. No se certifican asistencia, horas o aceptación de negocio.
