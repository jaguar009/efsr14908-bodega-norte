# Integración continua y publicación

El plan Word está en docs/word/12_Plan_Integracion_Continua.docx. ci.yml ejecuta las pruebas locales, construye GitHub Pages y Visual Studio, compila .NET y Java. deploy-pages.yml publica el frontend. Render despliega el backend automáticamente desde master.

Las pruebas PostgreSQL e IBM real se ejecutan por separado con configuración privada, sin exponer secretos en CI. Los datos se recuperan en un esquema nuevo y las versiones de código se gestionan mediante Git. Ambos workflows de la versión b31a431 terminaron correctamente.
