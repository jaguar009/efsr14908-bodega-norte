# Sistema Bodega Norte

Sistema web de inventario y ventas para una bodega simulada, preparado como proyecto de **Experiencias Formativas en Situaciones Reales de Trabajo III (EFSR14908)**.

## Equipo

Grupo 10. Coordinador: Gonzalo Yoel Choque Becerra. Integrantes: Adrian Alexander Berrocal Villanueva, Lalo Alessandro Eugenio Montalvo y Juan Diego Romero Peralta. Bodega Norte es un caso ficticio; los productos y ventas incluidos son datos de demostración.

## Qué resuelve

Bodega Norte centraliza el catálogo de productos, las existencias, las ventas de mostrador, los proveedores y los reportes operativos. El flujo principal es seleccionar productos, cobrar una venta y descontar automáticamente el stock. El proyecto incluye un modo demo con `localStorage` y un modo MVC con ASP.NET Core 8 y Supabase PostgreSQL.

## Funcionalidades implementadas

- Panel de inicio con ventas del día, stock total, alertas y actividad reciente.
- Punto de venta con búsqueda, categorías, carrito, cantidades y medios de pago.
- Registro y edición de productos con stock mínimo, costo, precio y proveedor.
- Inventario con filtros, estados de stock, edición y eliminación.
- Catálogo de productos y cálculo de margen estimado.
- Gestión de proveedores persistente, con vínculos y restricciones de eliminación.
- Reportes con indicadores, ranking, medios de pago y exportación CSV.
- Configuración de la bodega y modo oscuro.
- Diseño responsive para escritorio, tablet y celular.

Se implementaron cuentas Supabase Auth, permisos administrador/cajero, auditoría, ajustes de stock, anulaciones, constancias internas y respaldo JSON. Compras, contabilidad, facturación SUNAT y Watson quedan fuera del alcance; el asistente es local. El backend compartido aún requiere la conexión privada de PostgreSQL y validación del despliegue.

## Ejecución rápida en modo demo

```bash
npm install
npm run dev
```

Luego abrir la URL que muestre Vite, normalmente `http://localhost:5173`.

Para validar la compilación de producción:

```bash
npm run build
```

Para el modo estático de GitHub Pages y el host de Visual Studio se usan `npm run build:github` y `npm run build:visualstudio`.

Este modo usa datos de demostración en el navegador y es el que se publica en GitHub Pages.

## Ejecución con Visual Studio y Supabase PostgreSQL

La solución `backend/BodegaNorte.sln` contiene una aplicación ASP.NET Core 8 organizada con MVC. React es la vista visual; los `Controllers` reciben las solicitudes, `BodegaService` concentra las reglas de negocio, los `Models` representan los datos y `BodegaRepository` usa Npgsql para consultar PostgreSQL en Supabase.

Para ejecutar el modo MVC con Visual Studio y Supabase:

1. El proyecto **Bodega Norte Grupo 10** (`pladdberkeewsvomkset`, región `sa-east-1`) de la nueva cuenta ya tiene aplicado `database/schema.sql` y cargado `database/seed.sql`. Si necesitas reconstruirlo en otro proyecto, ejecuta schema.sql, upgrade_operations.sql y seed.sql, en ese orden. La migración additive upgrade_operations.sql ya fue aplicada al proyecto actual sin borrar los datos.
2. En Visual Studio, abre **Manage User Secrets** para `BodegaNorte.Api` y agrega esta estructura, reemplazando los campos con los valores de **Connect → Session pooler** y tu contraseña de base de datos:

```json
{
  "ConnectionStrings": {
    "BodegaNorte": "Host=HOST_DEL_SESSION_POOLER;Port=5432;Database=postgres;Username=USUARIO_DEL_POOLER;Password=TU_CONTRASEÑA;SSL Mode=Require"
  },
  "BootstrapAdminEmail": "CORREO_CONFIRMADO_DEL_ADMINISTRADOR",
  "Supabase": {
    "Url": "https://pladdberkeewsvomkset.supabase.co",
    "PublishableKey": "sb_publishable_TU_CLAVE"
  }
}
```

La misma URL y clave publishable se usan en el cliente web. Copia esos valores desde **Connect** del proyecto a `.env.visualstudio` (`VITE_SUPABASE_URL` y `VITE_SUPABASE_PUBLISHABLE_KEY`). Para desarrollo con Vite, colócalos en `.env.local` junto con `VITE_API_MODE=supabase` y `VITE_API_BASE_URL=http://localhost:5248/api`.

No guardes contraseñas ni claves en `appsettings.json` o Git. La clave publishable puede ir en el frontend; nunca pongas una clave `sb_secret` o `service_role` en una variable `VITE_`. Si no recuerdas la contraseña de base de datos, restablécela en Supabase y usa la nueva solo en User Secrets.
3. En **Authentication → URL Configuration**, agrega el origen de la aplicación (por ejemplo, `http://localhost:5248` y `http://localhost:5173`) a la lista de redirect URLs. Si publicas el sistema, agrega también el origen de producción.
4. Desde la raíz del proyecto, ejecuta `pnpm install --frozen-lockfile` y `npm run build:visualstudio`.
5. Abre `backend/BodegaNorte.sln` en Visual Studio y presiona **F5** sobre `BodegaNorte.Api`.
6. Comprueba `http://localhost:5248/api/health/ready` y abre la página que inicia Visual Studio.

La interfaz identifica este modo como **Supabase PostgreSQL**. Inicia sesión o crea una cuenta desde `#/login` y `#/signup`; el panel y sus secciones viven bajo `#/app/*`. React envía un bearer token a los controladores MVC, que consultan Supabase Auth para validar la sesión antes de permitir las operaciones de productos y ventas. El servicio valida las operaciones y el repositorio Npgsql consulta el esquema privado `bodega_norte`; las ventas y el descuento de stock se guardan dentro de una transacción PostgreSQL. Solo las cuentas habilitadas en members con rol admin o cashier pueden operar. La cuenta confirmada de BootstrapAdminEmail se habilita como administrador inicial; las demás quedan pendientes hasta que el administrador les asigne un rol en Configuración. El cajero consulta y registra ventas. Solo el administrador cambia productos, proveedores, stock, permisos, configuración, anulaciones y respaldos. No se toman roles desde user_metadata.

La integración de venta y actualización de stock se puede verificar con `integration/JavaInventoryIntegrationTest.java`, únicamente contra un proyecto o rama de pruebas de Supabase con datos de demostración. No ejecutes esa prueba contra la base principal, porque registra una venta en la base conectada.

## Publicación en GitHub Pages

El workflow `.github/workflows/deploy-pages.yml` construye el modo demo y lo publica automáticamente en GitHub Pages cuando se actualiza `master` o `main`. GitHub Pages solo ejecuta la interfaz estática; no publica las credenciales ni el backend MVC.

## Entregables académicos en Word

| Entregable | Momento de referencia | Contenido |
|---|---:|---|
| 01 Entregable inicial | Próxima semana, según anuncio de la docente | Capítulos I y II, diagnóstico, objetivos, justificación, alcance y plan inicial |
| 02 Avance del proyecto | Semana 10 | Avance funcional mínimo del 50 %, arquitectura, historias, módulos base y pruebas |
| 03 Entrega final | Semana 13 | Sistema concluido, informe, componentes, recursos, pruebas, manuales y sustentación |

Los informes y manuales editables están en `docs/word/`. Se prepararon en Arial 11, interlineado simple, tamaño A4 y blanco y negro. El informe final sigue la estructura del Anexo 4 Nivel 2 e incluye análisis SEPTE con notas al pie, objetivos SMART, Canvas, alcance, riesgos, viabilidad, gestión, Gantt, producto, recursos, conclusiones, recomendaciones, glosario, bibliografía y anexos.

El anuncio del Entregable 1 indica del 21 al 27 de septiembre de 2026. El Anexo 4 Nivel 2 ubica el avance en la semana 10 y el final en la 13; el sílabo 2026 ubica la evaluación final en la semana 7. `docs/word/04_Cronograma.docx` conserva la diferencia y deja la fecha final para confirmación en el aula virtual.

El registro del 04-10-2026 distingue 18 pruebas de lógica y demostración local, compilación .NET/Java, revisión funcional del navegador y la integración HTTP PostgreSQL aún pendiente. La aceptación de negocio del equipo sigue pendiente. La distribución de roles proviene del Entregable 1; no se atribuyen tareas individuales que no estén registradas.

La hoja `docs/entregables/Anexo1A_EFSRT_Grupo10_Avances1y2.xlsx` identifica al Grupo 10 y contiene propuestas de actividades para los avances 1 y 2. Las actividades son propuestas por ratificar; las fechas y horas se dejaron vacías porque no se proporcionaron registros reales. Completar también sede, monitor y códigos cuando el equipo cuente con esos datos.

## Estructura del proyecto

```text
EFSR14908_Bodega_Norte/
├─ src/                         Aplicación React y estilos
├─ public/                      Recursos visuales
├─ database/                    Esquema y datos iniciales de Supabase PostgreSQL
│  ├─ schema.sql                Tablas y relaciones
│  ├─ seed.sql                  Datos iniciales de demostración
│  └─ README.md                 Pasos de configuración local
├─ backend/BodegaNorte.sln      Solución para Visual Studio
│  └─ BodegaNorte.Api/          Aplicación ASP.NET Core 8 MVC
│     ├─ Controllers/           Controladores MVC y endpoints JSON
│     ├─ Models/                Modelos y contratos de entrada/salida
│     ├─ Services/              Reglas de negocio de Bodega Norte
│     ├─ Data/                  Repositorio Npgsql para PostgreSQL
│     └─ Views/                 Vista host MVC para la aplicación React
├─ docs/
│  ├─ entregables/              Informes por hito y hoja de seguimiento del Grupo 10
│  ├─ word/                     Informes y documentación editables en Word
│  ├─ diagramas/                Arquitectura y modelo de datos
│  ├─ manual_usuario.md         Recurso para el usuario final
│  ├─ guia_instalacion.md       Configuración y ejecución
│  ├─ manual_tecnico.md         Componentes y decisiones técnicas
│  ├─ matriz_pruebas.md         Plan y resultados de pruebas
│  ├─ cronograma.md             Gantt del proyecto
│  ├─ gestion/                  Backlog y plan de integración continua
│  ├─ guion_sustentacion.md     Guion de presentación
│  └─ integraciones/            Contrato de integración con Watson Assistant
└─ package.json
```

## Verificación técnica

- `npm run build`: compila la interfaz.
- `dotnet build backend/BodegaNorte.sln`: compila el backend MVC con Npgsql.
- `javac integration/JavaInventoryIntegrationTest.java`: compila la prueba HTTP.
- `java -cp integration JavaInventoryIntegrationTest http://localhost:5248`: verifica venta, descuento de stock y rechazo de venta excesiva en una base Supabase de prueba.

El workflow `.github/workflows/ci.yml` ejecuta las compilaciones de frontend y backend y compila la prueba Java. No ejecuta operaciones contra una base persistente.

## Decisiones técnicas y alcance

La interfaz se implementa con React + Vite y se aloja en `Views/Home/Index.cshtml`. En el modo MVC, `HomeController`, `ProductsController` y `SalesController` atienden las rutas; `BodegaService` valida las operaciones y `Data/BodegaRepository.cs` ejecuta las transacciones mediante Npgsql contra Supabase PostgreSQL. El modo demo conserva `localStorage` para una demostración sin servidor; Visual Studio activa las llamadas API al construir con `VITE_API_MODE=supabase`.

## Actualización operativa del 04-10-2026

- Cobros con identificador de reintento; transacción, bloqueo de stock y precio confirmado en el backend.
- Ajustes con motivo y versión; archivado de productos; cancelación devuelve stock una sola vez.
- Reportes por fecha de Lima, detalle real y exclusión de anulaciones. Los costos históricos desconocidos se identifican.
- Proveedores, configuración y roles persistentes; historial y auditoría; CSV y respaldo JSON.
- Pruebas: `pnpm test`, `pnpm run build:github`, `pnpm run build:visualstudio`, `dotnet build backend/BodegaNorte.sln` y compilación Java.
- Evidencia actual: `docs/evidencias/validacion_tecnica_2026-10-04.md`.
- GitHub Pages conserva el modo demo hasta que Render y la autenticación compartida estén verificados.
