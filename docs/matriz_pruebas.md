# Matriz de validación — 04-10-2026

## Ejecutadas técnicamente por Codex

| Caso | Resultado | Ambiente |
|---|---|---|
| 18 pruebas Node: fechas, informes vacíos, stock, reintentos, anulación, versiones, proveedores, configuración, CSV | Aprobadas, 18/18 | Lógica y demo local |
| build:visualstudio | Aprobado | Vite local |
| dotnet build | Aprobado, 0 errores y 0 advertencias | .NET 8 local |
| javac UTF-8 | Aprobado | Java local, no prueba HTTP |
| Carrito excede stock | Rechazado, cantidad previa conservada | Navegador demo |
| Limpiar con dos productos | Carrito completo vacío | Navegador demo |
| Venta de leche, Yape, total S/ 4.50 | Historial y constancia; stock 3 → 2 | Navegador demo |
| Anular la venta anterior con motivo | Movimiento +1 y saldo 3 | Navegador demo |
| Crear proveedor QA y archivar | Operaciones visibles | Navegador demo |
| Configuración: editar, actualizar y recargar | Edición conservada; guardado persistente | Navegador demo |
| Simular cajero | Sin edición de catálogo ni administración de configuración | Navegador demo, simulación |
| Cambiar rango a día sin ventas | S/ 0.00 y cero operaciones | Navegador demo |

## Pendientes

- Verificar la publicación de GitHub Pages tras el build local correcto.
- Aceptación móvil del equipo: la revisión técnica a 390 px (375 px útiles) ya mostró ancho del documento igual al viewport, sin desbordamiento horizontal.
- Integración HTTP PostgreSQL: token, roles, venta/reintento, exceso, ajuste concurrente y cancelación en una base desechable.
- Disponibilidad y sesión real en Render/GitHub Pages; recuperación por correo.
- Abrir CSV en Excel, imprimir constancia y ensayar recuperación del respaldo.
- Aceptación del Grupo 10 y de un usuario de negocio. No se incluyen firmas ni validadores inventados.

La prueba SQL Server del 28-09-2026 se mantiene como antecedente y no valida PostgreSQL. Véase evidencias/validacion_tecnica_2026-10-04.md.
