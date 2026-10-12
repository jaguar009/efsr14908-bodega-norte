# Instalación final

La guía Word completa está en docs/word/06_Guia_Instalacion.docx. Desde la raíz: pnpm install --frozen-lockfile y pnpm run dev para demo. Para MVC, pnpm run build:visualstudio y abrir backend/BodegaNorte.sln en Visual Studio con ASP.NET y .NET 8.

Configure ConnectionStrings:BodegaNorte, Supabase:Url, Supabase:PublishableKey y BootstrapAdminEmail mediante User Secrets. Nunca guarde conexiones privadas en archivos distribuidos. Para una base nueva, aplique schema.sql, upgrade_operations.sql y seed.sql; no siembre de nuevo una base operativa.

Las pruebas aisladas se ejecutan con dotnet run --project backend/BodegaNorte.Tools -c Release -- verify --report output/validacion_final.json. Java debe estar en PATH. La recuperación usa restore RESPALDO_JSON --schema bodega_recovery_NOMBRE y exige un esquema nuevo. Las cuentas y membresías no se restauran.

IBM: Watson:Enabled, Watson:ServiceUrl, Watson:AssistantId, Watson:EnvironmentId y Watson:ApiKey. Obtenga la URL específica de Assistant builder, no la URL general de Orchestrate. La clave es privada. En Render se usa __ para los niveles de configuración.
