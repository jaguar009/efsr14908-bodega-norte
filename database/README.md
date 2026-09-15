# SQL Server local

1. En SQL Server Management Studio, crea una base llamada `BodegaNorte`.
2. Ejecuta `schema.sql` dentro de esa base.
3. Ejecuta `seed.sql` para cargar categorías, proveedores, productos y ventas de demostración.
4. Si utilizas una instancia distinta a LocalDB, modifica la cadena `BodegaNorte` en `backend/BodegaNorte.Api/appsettings.Development.json`.

La cadena predeterminada utiliza LocalDB de Visual Studio:

```text
Server=(localdb)\MSSQLLocalDB;Database=BodegaNorte;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

Para SQL Server Express puede usarse, por ejemplo, `Server=.\SQLEXPRESS;...`. No subas contraseñas al repositorio.
