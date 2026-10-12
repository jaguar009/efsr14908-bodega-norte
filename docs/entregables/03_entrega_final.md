# Entrega final Bodega Norte

El informe Word está en docs/word/03_Entrega_Final_Informe.docx y la evidencia en docs/word/Anexo_Validacion_Final_Grupo10.docx. El caso es académico y utiliza datos ficticios. El sistema está publicado en GitHub Pages, Render y Supabase.

La versión final implementa catálogo, proveedores, venta idempotente, ajustes, anulaciones, reportes de Lima, permisos, auditoría, respaldo y recuperación en un esquema nuevo. Watson Assistant se conecta mediante Assistant builder de watsonx Orchestrate en Trial gratuito de 30 días. La ayuda local es independiente.

Pasaron 25 pruebas de dominio, 17 casos HTTP y PostgreSQL aislados, 11 recorridos de interfaz y 8 consultas contra IBM real. Los 17 casos usan fixtures de Auth e IBM; no se confunden con la comprobación IBM real. El programa Java se ejecutó contra la API aislada. La base principal permaneció sin cambios. La aceptación humana y las horas de asistencia se registran con datos reales del equipo.
