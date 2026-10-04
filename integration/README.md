# Integración HTTP Java

JavaInventoryIntegrationTest.java comprueba acceso anónimo rechazado, perfil autorizado, una venta idempotente, stock descontado una vez, exceso rechazado y anulación repetida que devuelve stock una vez (administrador). Un cajero debe recibir 403 al anular.

Ejecutar solo contra una base o esquema desechable de desarrollo con datos sembrados y el producto P-001 con stock suficiente. La prueba escribe ventas y auditoría. La cuenta debe existir en Supabase Auth, estar confirmada y tener rol en members del esquema de pruebas. No sustituye pruebas de concurrencia entre procesos.

1. Configurar backend Database:Schema con el esquema aislado creado expresamente para la prueba; aplicar allí los tres scripts SQL sustituyendo el nombre del esquema.
2. Iniciar sesión en la aplicación de pruebas con una cuenta habilitada. Obtener su token de sesión para BODEGA_API_TOKEN mediante un procedimiento privado de desarrollo; no guardar el token en Git ni en informes.
3. Configurar BODEGA_API_TOKEN en el entorno del proceso y opcionalmente BODEGA_API_URL.

```powershell
javac -encoding UTF-8 integration/JavaInventoryIntegrationTest.java
java -cp integration JavaInventoryIntegrationTest http://localhost:5248
```

Sin BODEGA_API_TOKEN el programa termina explicando el requisito. No imprime el token. Guardar solo resultados y fecha en docs/evidencias. El programa compiló el 04-10-2026; la prueba HTTP PostgreSQL está pendiente.
