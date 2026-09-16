# Guion de sustentación

## 1. Problema y oportunidad

“Bodega Norte tenía dificultad para conocer su stock real, registrar ventas con rapidez y decidir qué productos reponer. La oportunidad fue digitalizar el catálogo, la venta y las alertas en un único flujo.”

## 2. Solución

“Desarrollamos una aplicación web con Inicio, Ventas, Inventario, Productos, Proveedores y Reportes. El punto central es que una venta confirmada descuenta el stock y actualiza la información operativa.”

## 3. Demostración

1. Mostrar indicadores en Inicio.
2. Abrir Ventas y buscar “Leche Gloria”.
3. Agregar dos productos al carrito.
4. Cobrar con Yape.
5. Mostrar el aviso de venta registrada.
6. Abrir Inventario y comprobar el descuento.
7. Abrir Reportes y exportar CSV.

## 4. Diseño técnico

"La solución aplica MVC estrictamente: React y Views/Home/Index.cshtml forman la vista, los controladores ASP.NET Core reciben las operaciones, BodegaService aplica las reglas y BodegaRepository persiste el modelo en SQL Server LocalDB. También existe un modo demo con LocalStorage para ejecutar la interfaz sin servidor."

## 5. Validación y cierre

“Validamos el catálogo, el stock bajo, el carrito, el cobro, la persistencia, la exportación y el responsive. El sistema cumple el flujo principal de una bodega y queda preparado para autenticación, vencimientos y facturación electrónica.”
