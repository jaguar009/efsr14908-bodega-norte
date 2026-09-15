# Manual de usuario

## Ingreso

Ejecuta `npm run dev` y abre la URL indicada por Vite. La aplicación inicia en el panel **Inicio** con datos de demostración.

## Registrar una venta

1. Entra a **Ventas**.
2. Busca un producto o usa las categorías.
3. Haz clic en un producto para agregarlo al carrito.
4. Ajusta la cantidad con `+` y `-`.
5. Pulsa **Cobrar venta**.
6. Selecciona el medio de pago.
7. Pulsa **Registrar venta**.

El sistema descuenta las unidades del inventario y agrega la operación a la actividad reciente.

## Agregar o editar productos

En **Productos** o **Inventario**, pulsa **Agregar producto** o el ícono de edición. Completa nombre, categoría, unidad, stock, mínimo, proveedor, costo y precio. El botón de guardado se habilita cuando hay nombre y precio.

## Revisar stock bajo

En **Inicio** se muestran los productos que tienen stock igual o menor al mínimo. En **Inventario**, usa el filtro **Bajo** para concentrarte en ellos.

## Consultar reportes

En **Reportes** revisa ventas acumuladas, unidades, margen estimado, productos por reponer y medios de pago. **Exportar CSV** descarga el inventario actual.

## Datos y restablecimiento

Los datos se guardan en el navegador. Para volver a los datos iniciales, borra el almacenamiento local del sitio desde las herramientas del navegador y recarga la página.
