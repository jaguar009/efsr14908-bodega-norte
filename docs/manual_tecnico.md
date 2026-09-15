# Manual técnico

## Tecnologías

- React para composición de la interfaz y estado local.
- Vite para desarrollo y compilación.
- CSS nativo con variables de diseño y media queries.
- SVG inline para iconografía consistente.
- LocalStorage versionado para persistencia de la demo.
- SQL Server como modelo de persistencia de referencia.

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

## Extensión a backend

El modelo de datos contiene las entidades `categories`, `suppliers`, `products`, `sales` y `sale_items`. La interfaz actual puede reemplazar `localStorage` por endpoints REST sin cambiar el contrato funcional. La validación de permisos y la autenticación deben vivir en la API.

## Buenas prácticas aplicadas

- Estado inicial separado de los componentes visuales.
- Actualizaciones inmutables de productos, ventas y carrito.
- Cálculos derivados con `useMemo`.
- Persistencia versionada con manejo de errores.
- Componentes enfocados y CSS responsive.
