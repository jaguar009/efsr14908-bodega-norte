# Informe de avance del proyecto

El documento editable de avance está en `docs/word/02_Entregable_2_Avance_50.docx`. Resume el estado documentado al 4 de octubre de 2026 y amplía las capturas el 11 de octubre; no certifica el porcentaje que el Grupo 10 presentó en una semana específica.

## Alcance comprobado

- Interfaz React para inicio, productos, ventas, inventario, reportes y configuración.
- API ASP.NET Core con catálogo de productos y registro transaccional de ventas.
- Backend MVC con Npgsql y scripts Supabase PostgreSQL para datos de demostración.
- Compilación React para Visual Studio y backend ASP.NET Core 8 sin errores el 28 de septiembre de 2026.
- El proyecto Supabase contiene el esquema y los datos de demostración; la cadena está configurada en User Secrets y Render. La prueba HTTP de venta/stock en un esquema aislado permanece pendiente.

## Pendiente del equipo

- Aceptación manual de interfaz, responsive y exportación CSV.
- Registrar ejecutor, fecha y capturas reales de aceptación.
- Confirmar con la docente qué semana y fecha aplican al hito final.
- Sustituir datos de demostración solo con autorización del negocio, si se valida el caso.
- Verificar las operaciones de escritura del API en una base o esquema desechable de desarrollo.

## Actualización al 04-10-2026

La versión incorpora roles administrador/cajero, ajustes con motivo, venta idempotente, anulación, detalle y constancia interna, proveedores/configuración persistentes, auditoría y respaldo JSON. Pasaron 18 pruebas locales. El 4 de octubre se verificaron el despliegue compartido, la conexión de PostgreSQL y el ingreso del administrador. Las pruebas de escritura autenticada y la aceptación del equipo permanecen pendientes. No se certifican asistencia, horas o aceptación de negocio.

## Ampliación de evidencias del 11 de octubre de 2026

El informe Word contiene seis capturas en lugar de dos: escritorio, móvil, inventario, preparación de venta, proveedores y reportes. Las cuatro nuevas se obtuvieron en el modo de demostración local y están identificadas con su fecha, descripción y contexto. Las capturas están a color; el texto conserva Arial 11 y color negro. Los originales y sus versiones monocromas anteriores están en docs/evidencias/avance50_2026-10-11/.

## Anexo de evidencias técnicas

El registro detallado se adjunta en Anexo_Evidencias_Tecnicas_Grupo10.docx e incluye las validaciones históricas del 28 de septiembre y del 4 de octubre de 2026.
