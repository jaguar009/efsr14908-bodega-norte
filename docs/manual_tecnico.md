# Manual técnico

## Tecnologías

- React para la vista visual y composición de la interfaz.
- Vite para desarrollo y compilación.
- ASP.NET Core 8 MVC para controladores, vista host y configuración del servidor.
- CSS nativo con variables de diseño y media queries.
- SVG inline para iconografía consistente.
- Microsoft.Data.SqlClient para acceso a SQL Server local.
- LocalStorage versionado únicamente para el modo demo.

## Componentes principales

- `App`: shell, navegación, estado y composición de páginas.
- `Dashboard`: indicadores, gráfico, actividad y alertas.
- `Sales`: catálogo, categorías, carrito y cobro.
- `Inventory`: filtros, estados y acciones CRUD.
- `Products`: catálogo comercial y margen.
- `Suppliers`: proveedores y resumen de abastecimiento.
- `Reports`: indicadores y exportación CSV.
- `Settings`: datos de la bodega y preferencias.
- `ProductModal` y `CheckoutModal`: flujos de edición y confirmación.

## Reglas de negocio

1. Un producto está en stock bajo cuando `stock <= minStock`.
2. Una venta no permite agregar más unidades que el stock disponible.
3. Confirmar una venta agrega el registro y descuenta las cantidades.
4. El margen estimado se calcula como `1 - costo / precio`.
5. El inventario exportado incluye código, producto, categoría, stock, mínimo, precio y proveedor.

## Arquitectura MVC estricta

El modo de ejecución con Visual Studio sigue MVC de forma explícita:

- **View:** `Views/Home/Index.cshtml` aloja los archivos compilados de React; los componentes React presentan Dashboard, Ventas, Inventario y Reportes.
- **Controller:** `Controllers/HomeController.cs`, `ProductsController.cs`, `SalesController.cs` y `HealthController.cs` reciben las solicitudes y devuelven vistas o respuestas JSON.
- **Model:** `Models/Contracts.cs` define los contratos de entrada y salida; `Services/BodegaService.cs` aplica las reglas del negocio y `Data/BodegaRepository.cs` representa y persiste la información en SQL Server.

El controlador no ejecuta SQL directamente y la vista no accede a SQL Server. El flujo es `View → Controller → Service/Model → Repository → SQL Server`, con la respuesta retornando por el mismo circuito.

## Extensión a backend

El modelo de datos contiene las entidades `categories`, `suppliers`, `products`, `sales` y `sale_items`. El modo MVC ya expone los endpoints `/api/products`, `/api/sales` y `/api/health`; el modo demo utiliza `localStorage` solo para permitir una ejecución sin servidor. La validación de permisos y la autenticación deben vivir en los controladores y servicios del backend.

## Buenas prácticas aplicadas

- Estado inicial separado de los componentes visuales.
- Actualizaciones inmutables de productos, ventas y carrito.
- Cálculos derivados con `useMemo`.
- Persistencia versionada con manejo de errores.
- Componentes enfocados y CSS responsive.
