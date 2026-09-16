# Guía de instalación

## Requisitos

- Node.js 20.19 o superior (o Node.js 22.12 o superior).
- npm 9 o superior.
- Navegador actualizado.

## Instalación

```bash
cd EFSR14908_Bodega_Norte
npm install
npm run dev
```

Abre la dirección local que muestre Vite.

## Compilación

```bash
npm run build
npm run preview
```

## Ejecución MVC con Visual Studio y SQL Server local

1. Asegura que esté instalado el SDK de .NET 8, Visual Studio con ASP.NET y desarrollo web, y SQL Server LocalDB.
2. Ejecuta `database/schema.sql` y luego `database/seed.sql` en `(localdb)\\MSSQLLocalDB` o ajusta `backend/BodegaNorte.Api/appsettings.Development.json`.
3. Ejecuta `pnpm install` y `pnpm run build:visualstudio` para generar la vista React dentro de `backend/BodegaNorte.Api/wwwroot`.
4. Abre `backend/BodegaNorte.sln` en Visual Studio y ejecuta el proyecto `BodegaNorte.Api` con F5.
5. Verifica `http://localhost:5248/api/health` y abre la dirección de la aplicación.

La implementación usa `Views/Home/Index.cshtml` como vista host, los controladores de `Controllers/`, los contratos de `Models/`, `BodegaService` para reglas de negocio y `BodegaRepository` para SQL Server.

## Persistencia de demostración

El modo demo usa `localStorage` con las claves versionadas `bodega-norte:v1:products` y `bodega-norte:v1:sales`. El modo MVC usa SQL Server local mediante los controladores y repositorio del backend; no depende de `localStorage` para guardar productos ni ventas.

## Variables para integración futura

No se requieren variables para la demo. Para Watson Assistant o un backend real, usar un archivo `.env.local` no versionado y nunca escribir credenciales en el código fuente.
