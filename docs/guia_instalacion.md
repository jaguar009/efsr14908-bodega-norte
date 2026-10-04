# Instalación — Visual Studio MVC y Supabase

## Requisitos

Visual Studio con ASP.NET, SDK .NET 8, Node.js 22 y pnpm 11.19.0. Java 17 o posterior es opcional para integración. El repositorio contiene datos de una bodega simulada.

## Preparar la base

El proyecto actual es Bodega Norte Grupo 10 (pladdberkeewsvomkset), activo y con 12 productos y 6 ventas. La migración upgrade_operations.sql ya fue aplicada. Para instalar una base nueva ejecutar database/schema.sql, database/upgrade_operations.sql y database/seed.sql en ese orden. No volver a sembrar una base operativa como procedimiento de actualización.

## Guardar la conexión privada

1. Supabase Dashboard → proyecto → Connect → Session pooler.
2. Copiar la cadena y sustituir [YOUR-PASSWORD] por la contraseña PostgreSQL.
3. En Visual Studio: proyecto BodegaNorte.Api → Manage User Secrets.
4. Guardar ConnectionStrings:BodegaNorte, Supabase:Url, Supabase:PublishableKey y BootstrapAdminEmail.
5. Para una URI postgresql:// codificar caracteres reservados de la contraseña. También se admite formato Npgsql Host=...;Port=5432;Database=postgres;Username=...;Password=...;SSL Mode=VerifyFull.

El backend incluye el certificado raíz público oficial de Supabase en certificates/prod-ca-2021.crt, lo copia a la salida de compilación/publicación y lo usa automáticamente para sus hosts. Se verifican el certificado y el nombre del servidor. No contiene claves privadas. Si se indica otro Root Certificate en la conexión, debe existir en el servidor de ejecución.

El archivo privado local está en %APPDATA%/Microsoft/UserSecrets/BodegaNorte-EFSR14908-VisualStudio/secrets.json. No forma parte del repositorio. El MCP de Codex sirve para administrar Supabase y no reemplaza la conexión del backend.

## Construir y ejecutar

```powershell
pnpm install --frozen-lockfile
pnpm run build:visualstudio
dotnet build backend/BodegaNorte.sln
```

Abrir backend/BodegaNorte.sln y ejecutar BodegaNorte.Api con F5. Verificar http://localhost:5248/api/health/ready. Configurar en Supabase Auth las redirect URLs exactas: http://localhost:5248/ y http://localhost:5248/#/reset-password, además de las URLs de Vite que se utilicen. La cuenta confirmada de BootstrapAdminEmail entra como administrador; las demás requieren habilitación.

## Publicar

Render ya usa Dockerfile, plan gratuito y el espacio Juan Diego's workspace autorizado. La conexión privada y las variables Supabase__Url, Supabase__PublishableKey y BootstrapAdminEmail están configuradas. Backend: https://bodega-norte-grupo10.onrender.com/. Se verificó /api/health/ready con HTTP 200. .env.github usa VITE_API_MODE=supabase y VITE_API_BASE_URL=https://bodega-norte-grupo10.onrender.com/api. En Supabase Auth se guardaron la Site URL de Pages y cuatro destinos exactos: las raíces de Pages y Render y sus rutas #/reset-password. GitHub Pages solo aloja el frontend.

El servicio y el frontend están publicados. Falta crear/confirmar la cuenta inicial y comprobar el acceso autenticado y los flujos de administrador/cajero. El entorno gratuito puede suspenderse por inactividad.

## Comprobaciones

pnpm test ejecuta 18 pruebas locales. Los builds de ambos modos y .NET deben completar sin errores. integration/README.md explica la prueba HTTP Java con token de una cuenta habilitada y una base desechable. No está ejecutada aún contra PostgreSQL.
