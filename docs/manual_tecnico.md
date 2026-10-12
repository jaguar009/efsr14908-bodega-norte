# Manual técnico final

El documento completo está en docs/word/07_Manual_Tecnico.docx. React es la vista; ASP.NET Core MVC valida Auth y permisos; Npgsql usa el esquema privado bodega_norte de Supabase. Las ventas emplean ReadCommitted, bloqueos de filas y requestId. Los ajustes comprueban expectedVersion. La cancelación devuelve stock una sola vez.

El rol se obtiene de members y no de user_metadata. Las identidades sin correo confirmado se rechazan. La readiness verifica las nueve tablas y columnas de operación. Se valida TLS con VerifyFull.

BodegaNorte.Tools permite verificar y recuperar datos en esquemas nuevos; no importa miembros ni contraseñas. WatsonAssistantService obtiene tokens IAM temporales y aplica límite de entrada, tiempo y cuota. Solo envía la consulta, no registros de la base. La recuperación demo valida archivos demo-v2; el respaldo PostgreSQL usa postgresql-v2.
