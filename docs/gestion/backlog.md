# Backlog priorizado de Bodega Norte

El backlog traduce los flujos del caso de estudio en historias comprobables. Los estados describen lo que existe en el prototipo y no acreditan aceptación de usuarios reales.

| Prioridad | Historia de usuario | Criterio de aceptación | Estado del prototipo |
|---|---|---|---|
| Alta | Como administrador, quiero consultar productos, precios y existencias para conocer el catálogo disponible. | La vista y `GET /api/products` muestran productos con código, categoría, stock, mínimo y precio. | Implementado; aceptación visual pendiente. |
| Alta | Como cajero, quiero registrar una venta y elegir el medio de pago para cerrar una atención. | La API valida método y cantidad, guarda venta y líneas, calcula total y descuenta stock en una transacción. | Implementado y probado en demo; prueba HTTP PostgreSQL pendiente. |
| Alta | Como administrador, quiero impedir una venta con unidades superiores al stock. | La API devuelve error y no modifica el inventario si no hay unidades suficientes. | Implementado y probado en demo; prueba HTTP PostgreSQL pendiente. |
| Alta | Como responsable del inventario, quiero localizar productos por debajo del mínimo. | La aplicación destaca productos cuando `stock <= minStock`. | Implementado en la interfaz; aceptación visual pendiente. |
| Media | Como administrador, quiero exportar información del inventario para revisarla en una hoja de cálculo. | El módulo Reportes descarga un CSV con código, producto, categoría, stock, mínimo, precio y proveedor. | Implementado; aceptación manual pendiente. |
| Media | Como usuario, quiero consultar ayuda sobre operaciones comunes. | La demo ofrece respuestas locales para preguntas frecuentes sin exponer credenciales. | Asistente offline implementado; Watson no está conectado. |
| Baja | Como administrador, quiero operar con varias sucursales. | Cada usuario accede únicamente a funciones y datos de su rol y sucursal. | Fuera del alcance actual. |

## Revisión de una historia

Antes de cerrar una historia, registrar quién la revisó, fecha, ambiente, datos de prueba, resultado y evidencia. Si una historia requiere acceso a una organización real, obtener autorización antes de cargar datos o tomar capturas.

## Actualización al 04-10-2026

La versión incorpora roles administrador/cajero, ajustes con motivo, venta idempotente, anulación, detalle y constancia interna, proveedores/configuración persistentes, auditoría y respaldo JSON. Pasaron 18 pruebas locales; la integración HTTP PostgreSQL y el despliegue compartido siguen pendientes. Consultar docs/evidencias/validacion_tecnica_2026-10-04.md. No se certifican asistencia, horas o aceptación de negocio.
