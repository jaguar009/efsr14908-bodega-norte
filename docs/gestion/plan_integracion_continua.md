# Plan de integración continua

El repositorio incluye `.github/workflows/ci.yml`. El flujo se activa al subir cambios a `master` o `main` y al abrir una solicitud de integración. Comprueba ambos modos de compilación del frontend, compila la solución ASP.NET Core y verifica que el programa de integración Java compile.

## Secuencia del flujo

1. Descarga el código y configura Node.js 22 con pnpm.
2. Instala las dependencias desde el lockfile y construye las variantes GitHub Pages y Visual Studio.
3. Configura el SDK de .NET 8 y compila `backend/BodegaNorte.sln`.
4. Configura Java 17 y compila `integration/JavaInventoryIntegrationTest.java`.
5. Falla el trabajo si un comando termina con error; no publica el backend ni modifica una base de datos.

La publicación existente de GitHub Pages se mantiene en `.github/workflows/deploy-pages.yml` y solo despliega el modo estático de demostración. El backend ASP.NET Core requiere un host propio y la cadena de conexión Supabase en una configuración de secretos del entorno.

## Validaciones que todavía son manuales

- Ejecutar la prueba HTTP contra una rama o proyecto Supabase desechable, siguiendo `integration/README.md`.
- Revisar en navegador las ventas, filtros, exportación CSV y diseño responsive.
- Guardar el log y los datos del ambiente en `docs/evidencias/`.
- Evitar incorporar credenciales y datos de negocio reales en los flujos del repositorio.

## Criterio para cambios

Integrar cambios cuando ambas compilaciones terminen sin errores, la prueba Java compile y la revisión manual de las historias afectadas no reporte defectos bloqueantes. La integración HTTP se mantiene manual y se ejecuta solo contra una base Supabase de pruebas, no desde el workflow.

## Actualización al 04-10-2026

La versión incorpora roles administrador/cajero, ajustes con motivo, venta idempotente, anulación, detalle y constancia interna, proveedores/configuración persistentes, auditoría y respaldo JSON. Pasaron 18 pruebas locales; la integración HTTP PostgreSQL y el despliegue compartido siguen pendientes. Consultar docs/evidencias/validacion_tecnica_2026-10-04.md. No se certifican asistencia, horas o aceptación de negocio.
