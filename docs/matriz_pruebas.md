# Matriz de pruebas

| ID | Módulo | Caso | Resultado esperado | Resultado |
|---|---|---|---|---|
| PF-01 | Productos | Crear producto con nombre y precio válidos | El producto aparece en catálogo e inventario | Pendiente de ejecución del equipo |
| PF-02 | Productos | Guardar sin nombre o sin precio | El botón de guardado permanece deshabilitado | Pendiente de ejecución del equipo |
| PF-03 | Inventario | Filtrar por `Bajo` | Solo aparecen productos con stock menor o igual al mínimo | Pendiente de ejecución del equipo |
| PF-04 | Inventario | Editar stock mínimo | Cambia el estado del producto cuando corresponde | Pendiente de ejecución del equipo |
| PF-05 | Ventas | Agregar producto al carrito | Se muestra la línea y se recalcula el total | Pendiente de ejecución del equipo |
| PF-06 | Ventas | Agregar más unidades que el stock | Se muestra aviso y no se excede la disponibilidad | Pendiente de ejecución del equipo |
| PF-07 | Ventas | Confirmar venta en Efectivo | Se crea la venta y se descuenta stock | Pendiente de ejecución del equipo |
| PF-08 | Ventas | Confirmar venta en Yape, Plin o Tarjeta | Se guarda el medio de pago elegido | Pendiente de ejecución del equipo |
| PF-09 | Reportes | Exportar CSV | Se descarga `bodega-norte-inventario.csv` | Pendiente de ejecución del equipo |
| PF-10 | Persistencia | Recargar navegador después de editar | Los productos y ventas permanecen | Pendiente de ejecución del equipo |
| PF-11 | Responsive | Abrir en 390 px de ancho | No existe desbordamiento horizontal crítico | Pendiente de ejecución del equipo |
| IT-01 | Integración | Producto → carrito → venta | La venta usa el producto seleccionado | Pendiente de ejecución del equipo |
| IT-02 | Integración | Venta → inventario | El stock se descuenta exactamente por cantidad | Pendiente de ejecución del equipo |
| IT-03 | Integración | Venta → inicio | La actividad reciente muestra la operación | Pendiente de ejecución del equipo |
| IT-04 | Integración | Venta → reportes | Los totales se actualizan con la nueva venta | Pendiente de ejecución del equipo |

## Evidencia recomendada

Para cada caso, guardar captura con el nombre `EVID_<ID>_<modulo>.png` y anotar fecha, navegador, datos usados, resultado y observación. Las capturas deben organizarse en una carpeta `docs/evidencias/` para la entrega final.
