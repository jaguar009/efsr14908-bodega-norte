# Integración y validación

## Prueba Java

El archivo `JavaInventoryIntegrationTest.java` valida el caso IT-02 del flujo producto → venta → descuento de stock:

```bash
javac JavaInventoryIntegrationTest.java
java JavaInventoryIntegrationTest
```

Salida esperada:

```text
IT-02 OK: venta registrada y stock actualizado
```

La prueba es una evidencia técnica complementaria. La validación visual del flujo completo se realiza sobre la aplicación web.
