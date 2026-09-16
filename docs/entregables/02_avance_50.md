# Entregable 2 Avance del proyecto

## Criterio del hito

Para la semana 10, el proyecto debe demostrar al menos 50 % de avance y una sustentación grupal ante el monitor. La versión de este repositorio supera ese mínimo funcional con el flujo base de inventario, productos y ventas.

## Historias de usuario priorizadas

| ID | Historia | Criterio de aceptación | Estado |
|---|---|---|---|
| HU-01 | Como encargado quiero registrar productos para mantener el catálogo actualizado | El formulario valida nombre, stock, mínimo, costo y precio | Completada |
| HU-02 | Como encargado quiero consultar el inventario para identificar productos con stock bajo | La tabla permite buscar y muestra estado según el mínimo | Completada |
| HU-03 | Como cajero quiero buscar productos y agregarlos a una venta | El catálogo filtra y el carrito acumula cantidades | Completada |
| HU-04 | Como cajero quiero cobrar una venta con un medio de pago | La venta se registra y descuenta el stock | Completada |
| HU-05 | Como encargado quiero ver alertas para priorizar reposiciones | Inicio e inventario muestran productos bajo mínimo | Completada |
| HU-06 | Como encargado quiero revisar ventas recientes | Inicio y reportes muestran actividad y totales | Completada |
| HU-07 | Como encargado quiero exportar inventario | Se descarga un archivo CSV con los productos | Completada |
| HU-08 | Como encargado quiero consultar preguntas frecuentes de operación | Existe contrato para un asistente local/Watson | Diseñada |

## Módulos desarrollados en el avance

- **Inicio:** indicadores, gráfico de ventas, actividad reciente y alertas.
- **Ventas:** catálogo, búsqueda, categorías, carrito y cobro.
- **Inventario:** filtro, estados, edición y eliminación.
- **Productos:** catálogo, margen y formulario CRUD.
- **Persistencia:** modo demo con `localStorage` y modo MVC con SQL Server local.

## Arquitectura

La implementación utiliza estrictamente MVC, presentado en tres capas:

1. **View / Vista:** React + CSS responsive y `Views/Home/Index.cshtml` como vista host.
2. **Controller / Controlador:** `HomeController`, `ProductsController`, `SalesController` y `HealthController`; `BodegaService` concentra las reglas de negocio.
3. **Model / Modelo:** contratos en `Models/Contracts.cs`, `BodegaRepository` y SQL Server LocalDB para entidades y transacciones.

En el modo MVC, la vista nunca consulta SQL directamente: envía la operación al controlador y recibe el modelo serializado como respuesta JSON.

El diagrama se encuentra en `docs/diagramas/arquitectura.svg` y el modelo de datos en `docs/diagramas/modelo_datos.svg`.

## Metodología Scrum

| Rol | Responsabilidad |
|---|---|
| Product Owner | Priorizar necesidades de la bodega y validar criterios |
| Scrum Master | Facilitar reuniones, remover bloqueos y cuidar el cronograma |
| Desarrollo | Diseñar, programar, probar y documentar |
| Coordinador | Consolidar archivos y presentar el proyecto |

### Sprint propuesto

- Sprint 1: diagnóstico, alcance y entregable inicial.
- Sprint 2: diseño de historias, navegación y modelo de datos.
- Sprint 3: inventario y productos.
- Sprint 4: punto de venta y actualización de stock.
- Sprint 5: reportes, pruebas, manuales y cierre.

## Plan de pruebas

Se validan casos de catálogo, stock bajo, carrito, cobro, persistencia, exportación y responsive. La matriz completa se encuentra en `docs/matriz_pruebas.md`.

## Plan de integración continua

1. Cada cambio se registra en Git con un mensaje descriptivo.
2. Se ejecuta `npm run build` antes de integrar una funcionalidad.
3. Se revisan errores de consola y el flujo principal en navegador.
4. El equipo conserva un checklist de pruebas antes de cada entrega.
5. El despliegue web ejecuta `pnpm run build:github`; la ejecución local con Visual Studio usa `pnpm run build:visualstudio` y publica la vista dentro de `wwwroot`.

## Integración demostrable

El flujo de integración principal es: **seleccionar producto → agregar al carrito → confirmar cobro → registrar venta → descontar stock → reflejar alerta/reportes**. Esta integración se puede observar en una sola sesión de navegador y está cubierta por las pruebas IT-01 a IT-04.
