# Guion de sustentación

## 1. Problema y oportunidad

“El escenario ficticio Bodega Norte plantea dificultad para conocer su stock real, registrar ventas con rapidez y decidir qué productos reponer. La oportunidad fue digitalizar el catálogo, la venta y las alertas en un único flujo.”

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

"La solución organiza la ejecución de Visual Studio con MVC: React y Views/Home/Index.cshtml forman la vista, los controladores ASP.NET Core reciben las operaciones, BodegaService aplica las reglas y BodegaRepository usa Npgsql para persistir en Supabase PostgreSQL. También existe un modo demo con LocalStorage para ejecutar la interfaz sin servidor."

## 5. Validación y cierre

“Pasaron 18 pruebas locales y se revisaron los flujos de demostración del navegador. Se implementaron autenticación, roles, auditoría y respaldo JSON. La integración HTTP PostgreSQL, la recuperación del respaldo, la publicación compartida y la aceptación del equipo permanecen pendientes.”
