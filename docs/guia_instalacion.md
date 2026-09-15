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

## Persistencia de demostración

La aplicación usa `localStorage` con las claves versionadas `bodega-norte:v1:products` y `bodega-norte:v1:sales`. Esto evita requerir una base de datos para la demostración. El script `database/schema.sql` contiene el modelo SQL Server para una implementación con API.

## Variables para integración futura

No se requieren variables para la demo. Para Watson Assistant o un backend real, usar un archivo `.env.local` no versionado y nunca escribir credenciales en el código fuente.
