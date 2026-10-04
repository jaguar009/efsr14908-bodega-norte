# Registro de validación técnica y migración a Supabase

Fecha: 28 de septiembre de 2026
Ambiente: Windows, Node.js, .NET SDK 10 compilando proyecto .NET 8, Java 22.
Alcance: compilaciones anteriores y compilación actual del backend MVC con Npgsql.
Responsable: verificación técnica local para preparar el paquete; no sustituye la aceptación del Grupo 10.

## Compilaciones

- `npm run build`: correcto. Vite 8.3.0 transformó 16 módulos y generó `dist/`.
- `dotnet build backend\BodegaNorte.sln`: correcto antes de la migración, cero advertencias y cero errores.
- `javac integration\JavaInventoryIntegrationTest.java`: correcto.
- `npm run build:visualstudio`: correcto después de activar el modo Supabase; Vite 8.3.0 compiló 16 módulos.
- `dotnet build backend\BodegaNorte.sln`: correcto después de sustituir SQL Server por Npgsql 10.0.3, cero advertencias y cero errores.

## Prueba HTTP histórica de la versión SQL Server

Esta comprobación pertenece a la versión anterior del backend y no valida el repositorio Npgsql actual.

Se creó `BodegaNorte_EFSRT_Verificacion_20260928_01`, se aplicaron `database/schema.sql` y `database/seed.sql`, y se inició la API con una cadena de conexión temporal. La prueba registró una venta de una unidad de P-001, verificó el cambio de stock de 3.00 a 2.00 y solicitó una venta superior al stock. La segunda operación devolvió HTTP 400 y no modificó el stock. La base se eliminó al concluir la prueba.

Salida:

```text
IT-API-01 OK: venta registrada y stock actualizado 3.00 -> 2.00
IT-API-02 OK: venta superior al stock rechazada y stock conservado
```

## Migración a Supabase PostgreSQL

El repositorio MVC ya utiliza Npgsql y configuración mediante User Secrets. El 29 de septiembre de 2026 se creó en la cuenta Supabase nueva el proyecto `pladdberkeewsvomkset` (`sa-east-1`), se aplicó el esquema privado `bodega_norte` y se cargaron los datos de demostración. La consulta remota confirmó 7 categorías, 5 proveedores, 12 productos, 6 ventas y 15 detalles. `.env.visualstudio` apunta al proyecto nuevo. La conexión Npgsql de la API local y la prueba HTTP contra PostgreSQL siguen pendientes porque falta configurar la contraseña de base de datos en User Secrets. Esto no representa aceptación de interfaz, uso con usuarios reales ni conexión con Watson Assistant.
