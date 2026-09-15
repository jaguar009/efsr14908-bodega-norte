# Entrega final del proyecto

## Resumen final

Bodega Norte es una aplicación web para digitalizar inventario y ventas de una pequeña bodega. La versión final permite administrar productos, registrar ventas, actualizar existencias, detectar stock bajo, consultar proveedores y exportar información operativa.

## Componentes entregados

- Aplicación React + Vite en `src/`.
- Datos semilla para una demostración reproducible.
- Persistencia local versionada.
- Esquema SQL Server de referencia en `database/schema.sql`.
- Arquitectura y modelo de datos en SVG.
- Plan de pruebas y evidencias documentales.
- Guía de instalación y manual de usuario.
- Manual técnico y contrato de integración con Watson Assistant.

## Verificación funcional

El flujo principal validado es:

1. Abrir **Ventas**.
2. Buscar o seleccionar un producto.
3. Agregar varias unidades al carrito.
4. Confirmar el total y elegir Efectivo, Yape, Plin o Tarjeta.
5. Registrar la venta.
6. Revisar que el stock disminuya en **Inventario**.
7. Revisar la actividad en **Inicio** y los totales en **Reportes**.

## Conclusiones

1. La digitalización del catálogo y el registro de ventas reduce la dependencia de cálculos y anotaciones manuales.
2. El descuento automático de existencias conecta la venta con la reposición y hace visible el riesgo de quiebre de stock.
3. La estructura modular permite continuar con una API, SQL Server y autenticación sin rediseñar el flujo principal.

## Recomendaciones

1. Realizar una prueba piloto con datos reales durante una semana y comparar el inventario físico con el registrado.
2. En una siguiente versión, incorporar autenticación, copias de seguridad y control de vencimientos.
3. Confirmar con la bodega las reglas tributarias antes de integrar comprobantes o facturación electrónica.

## Glosario

- **Stock mínimo:** cantidad de referencia que activa una alerta de reposición.
- **Punto de venta:** módulo donde se seleccionan productos y se registra el cobro.
- **CRUD:** crear, consultar, actualizar y eliminar registros.
- **LocalStorage:** almacenamiento local del navegador para la demo.
- **API:** interfaz que permite que la aplicación se comunique con un servicio.
- **Sprint:** periodo corto de trabajo dentro de Scrum.

## Bibliografía y fuentes de trabajo

- IES CIBERTEC. *Anexo 4 - Informe de Proyecto ETI - Plan Nivel 2 para EFSRT III, IV V*.
- IES CIBERTEC. *Plan de Implementación de EFSRT - ETI 2024*.
- IES CIBERTEC. *Silabo 2026 04 EFSRT III*.
- Registro de observación del proceso de venta y reposición de Bodega Norte, por completar con fecha, responsable y evidencia del equipo.

## Anexos

- Capturas del panel, inventario, ventas, reportes y configuración.
- Exportación CSV de inventario.
- Matriz de pruebas.
- Diagramas.
- Guía de instalación y manual de usuario.
