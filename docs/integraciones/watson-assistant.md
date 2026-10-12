# Watson Assistant integrado

La integración real está en AssistantController y WatsonAssistantService. La instancia de IBM del Grupo 10 usa Assistant builder de watsonx Orchestrate en Sídney, Trial gratuito de 30 días. La versión 1 se publicó en Live.

Se configuraron ocho acciones en español y 32 ejemplos. El contenido está en integration/watson/bodega_norte_acciones.json y preguntas_frecuentes.json, sin claves. Las ocho consultas reales pasaron; consultar docs/evidencias/validacion_watson_2026-10-11.json y el documento Word 10_Integracion_Watson_Assistant.docx.

GET /api/assistant/status y POST /api/assistant/message exigen Staff. La entrada se limita a 500 caracteres y 20 consultas por minuto por usuario. La clave solo está en User Secrets o el entorno privado del backend. IAM produce un token temporal. No se envían registros de productos, ventas ni datos personales del perfil.

IBM orienta sobre el uso; no ejecuta operaciones. La ayuda local consulta stock y ventas. El Trial es temporal y requiere contratar un plan para continuar después de su vencimiento; el proyecto no cambia el plan automáticamente.
