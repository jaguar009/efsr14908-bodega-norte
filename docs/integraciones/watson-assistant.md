# Integración con Watson Assistant

## Objetivo

Incorporar un asistente para responder preguntas operativas frecuentes sin exponer información sensible. La demo actual conserva el flujo principal offline; este documento define el contrato para conectar Watson Assistant cuando el equipo tenga el servicio y las credenciales institucionales.

## Intenciones sugeridas

| Intención | Ejemplos |
|---|---|
| `consultar_stock` | “¿Cuánta leche queda?”, “¿Qué productos tienen stock bajo?” |
| `registrar_venta` | “¿Cómo registro una venta?”, “¿Cómo cobro con Yape?” |
| `consultar_reporte` | “¿Dónde veo las ventas de hoy?” |
| `ayuda_producto` | “¿Cómo agrego un producto?” |

## Entidades

- `producto`: nombre o código del producto.
- `medio_pago`: Efectivo, Yape, Plin o Tarjeta.
- `periodo`: hoy, semana, mes.

## Contrato de aplicación

```text
POST /api/assistant/message
{
  "message": "¿Qué productos tienen stock bajo?",
  "context": { "store": "Bodega Norte", "user": "cajera" }
}
```

La API debe devolver una respuesta legible y, si corresponde, una lista de productos o una acción sugerida. Las credenciales se guardan en variables de entorno del backend; no se agregan al repositorio.
