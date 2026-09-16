# Entregable 1 Informe inicial

## Datos del proyecto

- **Curso:** EFSR14908 Experiencia Formativa en Situación Real de Trabajo
- **Proyecto:** Sistema web de inventario y ventas para Bodega Norte
- **Modalidad:** Proyecto productivo dentro del IES
- **Nivel:** EFSRT III, Plan Nivel 2
- **Equipo:** 3 a 4 estudiantes
- **Coordinador:** Por completar por el equipo
- **Docente monitor:** Judith Jiménez Monago, según el anuncio compartido

## Resumen

Bodega Norte es una aplicación web orientada a una pequeña bodega que necesita controlar sus productos, registrar ventas y conocer cuándo debe reponer mercadería. La solución propone un punto de venta sencillo conectado con un inventario, alertas de stock bajo, gestión de proveedores y reportes exportables. El primer entregable define el problema, la oportunidad de mejora, los objetivos, los beneficiarios y el alcance que guiará el desarrollo.

## Introducción

Las bodegas atienden muchas operaciones pequeñas durante el día: reciben productos, consultan precios, venden por unidad, verifican existencias y realizan pedidos a proveedores. Cuando estas actividades se registran solo en cuadernos o archivos dispersos, el encargado puede desconocer el stock real, vender productos agotados o comprar sin priorizar las alertas más importantes.

El proyecto plantea una aplicación web con arquitectura MVC estricta que representa el proceso de negocio de la bodega. El usuario podrá consultar productos, registrar una venta, descontar cantidades automáticamente, visualizar indicadores y exportar información para tomar decisiones de reposición.

## Diagnóstico SEPTE

### Variable social

La bodega cumple una función de proximidad para los vecinos. La rapidez en la atención y la disponibilidad de productos influyen en la experiencia de compra. Una consulta de inventario más ordenada ayuda a responder con mayor certeza y evita ofrecer productos que ya no están disponibles.

### Variable económica

El margen de una bodega depende de controlar el precio de compra, el precio de venta, la rotación y las pérdidas por vencimiento o quiebre de stock. Un sistema que muestra costo, precio y margen estimado permite priorizar productos y pedidos.

### Variable tecnológica

Una aplicación web accesible desde una computadora o tablet reduce la dependencia de registros manuales. La interfaz debe ser rápida, legible y utilizable por una persona que atiende público, con búsqueda y registro de venta en pocos pasos.

### Variable ecológica

El uso de registros digitales reduce impresiones y facilita identificar productos inmovilizados o con baja rotación. En una siguiente iteración se puede agregar control de vencimientos y compras basadas en rotación para reducir desperdicio.

### Oportunidad de mejora

La oportunidad consiste en digitalizar el control de productos y ventas de una bodega sin exigir un proceso complejo. La solución debe priorizar tres resultados: stock confiable después de cada venta, alertas de reposición y reportes operativos comprensibles.

## Problema

La bodega registra productos, ventas y reposiciones de manera manual o dispersa. Esto produce diferencias entre el stock registrado y el real, demora para calcular el total de una venta, dificultad para identificar productos por debajo del mínimo y poca información para decidir qué comprar.

## Objetivos SMART

1. **Diseñar y desarrollar**, durante el periodo del proyecto, una aplicación web que permita registrar al menos 12 productos, clasificarlos por categoría y controlar stock actual y stock mínimo.
2. **Implementar**, antes de la entrega final, un flujo de venta con búsqueda, carrito, selección de medio de pago y descuento automático de existencias, validado con una matriz de pruebas funcionales.
3. **Generar**, antes de la sustentación final, reportes de ventas e inventario exportables a CSV para apoyar la reposición y el seguimiento del negocio.

## Justificación

La solución es aplicable porque concentra en un solo flujo las actividades que más afectan la operación diaria: saber qué hay, vender sin cálculos manuales, descontar stock y detectar faltantes. La propuesta tiene un alcance manejable para un equipo de estudiantes y permite evidenciar análisis, diseño, desarrollo, pruebas y documentación.

### Beneficiarios directos

- Encargado o propietario de Bodega Norte.
- Cajero que registra las ventas.
- Persona responsable de compras y reposición.
- Equipo que desarrolla y prueba la solución.

### Beneficiarios indirectos

- Clientes de la bodega, por una atención más rápida.
- Proveedores, por pedidos basados en necesidades reales.
- Vecinos, por una mayor disponibilidad de productos de uso frecuente.

## Modelo Canvas

| Bloque | Propuesta para Bodega Norte |
|---|---|
| Propuesta de valor | Inventario actualizado, venta rápida y alertas de reposición en una sola aplicación |
| Segmentos | Bodegas pequeñas con atención de mostrador |
| Canales | Aplicación web en computadora, tablet o laptop |
| Relación | Interfaz simple, manual de usuario y alertas visibles |
| Ingresos | Mejora del margen y reducción de pérdidas por faltantes o compras desordenadas |
| Recursos clave | Aplicación, catálogo, datos de productos, usuario cajero y proveedor |
| Actividades clave | Registrar productos, vender, controlar stock, revisar reportes y reponer |
| Socios clave | Proveedores y responsable de la bodega |
| Costos | Desarrollo, capacitación, mantenimiento y futura infraestructura de despliegue |

## Definición y alcance

### Incluido

- Catálogo de productos con código, categoría, unidad, costo, precio y proveedor.
- Inventario con stock actual, stock mínimo y estado de reposición.
- Punto de venta con búsqueda, categorías, carrito y medios de pago.
- Descuento de stock después de confirmar una venta.
- Historial de ventas y actividad reciente.
- Reportes visuales y exportación CSV.
- Gestión visual de proveedores.
- Configuración básica de la bodega.
- Manual de usuario, guía de instalación, manual técnico y matriz de pruebas.

### No incluido en la primera versión

- Facturación electrónica ante SUNAT.
- Integración bancaria real o cobro con POS físico.
- Control de lotes y vencimientos.
- Multi-sucursal y multi-almacén.
- Usuarios con autenticación contra un servidor.

### Riesgos y medidas

| Riesgo | Impacto | Medida |
|---|---|---|
| Datos de prueba incompletos | Medio | Usar catálogo semilla y validar casos extremos |
| Confusión entre stock mínimo y stock actual | Alto | Etiquetas claras y pruebas de venta con alerta |
| Pérdida de datos del navegador | Medio | Usar SQL Server en modo MVC y exportar CSV en modo demo |
| Alcance demasiado amplio | Alto | Priorizar inventario, ventas, reportes y recursos |
| Credenciales de servicios externos | Alto | No incrustar claves; usar variables de entorno |

## Productos comprometidos

1. Aplicación web ejecutable.
2. Código fuente organizado.
3. Esquema SQL Server de referencia.
4. Diagramas de arquitectura y datos.
5. Historias de usuario y backlog.
6. Plan y matriz de pruebas.
7. Guía de instalación y manual de usuario.
8. Manual técnico y contrato de integración con Watson Assistant.
9. Informe final editable y guion de sustentación.
