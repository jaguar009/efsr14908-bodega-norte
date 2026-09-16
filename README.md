# Sistema Bodega Norte

Sistema web de inventario y ventas para una bodega, desarrollado como proyecto del curso **EFSR14908 Experiencia Formativa en Situación Real de Trabajo**.

## Qué resuelve

Bodega Norte centraliza el catálogo de productos, las existencias, las ventas de mostrador, los proveedores y los reportes operativos. El flujo principal es seleccionar productos, cobrar una venta y descontar automáticamente el stock. El proyecto incluye un modo demo con `localStorage` y un modo MVC estricto con ASP.NET Core y SQL Server local.

## Funcionalidades implementadas

- Panel de inicio con ventas del día, stock total, alertas y actividad reciente.
- Punto de venta con búsqueda, categorías, carrito, cantidades y medios de pago.
- Registro y edición de productos con stock mínimo, costo, precio y proveedor.
- Inventario con filtros, estados de stock, edición y eliminación.
- Catálogo de productos y cálculo de margen estimado.
- Gestión visual de proveedores.
- Reportes con indicadores, ranking, medios de pago y exportación CSV.
- Configuración de la bodega y modo oscuro.
- Diseño responsive para escritorio, tablet y celular.

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

Este modo usa datos de demostración en el navegador y es el que se publica en GitHub Pages.

## Ejecución con Visual Studio y SQL Server local

La solución `backend/BodegaNorte.sln` contiene una aplicación ASP.NET Core 8 organizada estrictamente con MVC. React se utiliza como vista visual; los `Controllers` reciben las solicitudes, `BodegaService` concentra las reglas de negocio, los `Models` representan los datos y `BodegaRepository` encapsula el acceso a SQL Server.

Para ejecutar el modo MVC con Visual Studio y SQL Server local:

1. Crea la base `BodegaNorte` en LocalDB o SQL Server.
2. Ejecuta `database/schema.sql` y después `database/seed.sql`.
3. Ajusta la cadena de conexión en `backend/BodegaNorte.Api/appsettings.Development.json` si no usas LocalDB.
4. Ejecuta `pnpm install` y luego `pnpm run build:visualstudio`.
5. Abre `backend/BodegaNorte.sln` en Visual Studio y presiona **F5** sobre `BodegaNorte.Api`.
6. Comprueba `http://localhost:5248/api/health` y abre la página que inicia Visual Studio.

La interfaz identifica este modo como **SQL Server local** y las operaciones de productos y ventas se guardan mediante los controladores MVC y el repositorio SQL Server.

## Publicación en GitHub Pages

El workflow `.github/workflows/deploy-pages.yml` construye el modo demo y lo publica automáticamente en GitHub Pages cuando se actualiza `master` o `main`. GitHub Pages solo ejecuta la interfaz estática; por seguridad y por las limitaciones del hosting, no expone la instancia SQL Server local.

## Entregables académicos en Word

| Entregable | Momento de referencia | Contenido |
|---|---:|---|
| 01 Entregable inicial | Próxima semana, según anuncio de la docente | Capítulos I y II, diagnóstico, objetivos, justificación, alcance y plan inicial |
| 02 Avance del proyecto | Semana 10 | Avance funcional mínimo del 50 %, arquitectura, historias, módulos base y pruebas |
| 03 Entrega final | Semana 13 | Sistema concluido, informe, componentes, recursos, pruebas, manuales y sustentación |

Los informes y manuales editables para presentar están en `docs/word/`. El anuncio de la docente y el Anexo 4 son instrucciones de referencia distintas. El sílabo adjunto también menciona la evaluación final en la semana 7; por ello, la fecha efectiva debe confirmarse con la docente. El documento `04_Cronograma.docx` consolida los hitos y deja visible esta diferencia para evitar una entrega fuera de plazo.

## Estructura del proyecto

```text
EFSR14908_Bodega_Norte/
├─ src/                         Aplicación React y estilos
├─ public/                      Recursos visuales
├─ database/                    Modelo y datos iniciales para SQL Server
│  ├─ schema.sql                Tablas y relaciones
│  ├─ seed.sql                  Datos iniciales de demostración
│  └─ README.md                 Pasos de configuración local
├─ backend/BodegaNorte.sln      Solución para Visual Studio
│  └─ BodegaNorte.Api/          Aplicación ASP.NET Core 8 MVC
│     ├─ Controllers/           Controladores MVC y endpoints JSON
│     ├─ Models/                Modelos y contratos de entrada/salida
│     ├─ Services/              Reglas de negocio de Bodega Norte
│     ├─ Data/                  Repositorio y consultas SQL Server
│     └─ Views/                 Vista host MVC para la aplicación React
├─ docs/
│  ├─ entregables/              Informes separados por hito
│  ├─ word/                     Informes y documentación editables en Word
│  ├─ diagramas/                Arquitectura y modelo de datos
│  ├─ manual_usuario.md         Recurso para el usuario final
│  ├─ guia_instalacion.md       Configuración y ejecución
│  ├─ manual_tecnico.md         Componentes y decisiones técnicas
│  ├─ matriz_pruebas.md         Plan y resultados de pruebas
│  ├─ cronograma.md             Gantt del proyecto
│  ├─ guion_sustentacion.md     Guion de presentación
│  └─ integraciones/            Contrato de integración con Watson Assistant
└─ package.json
```

## Decisiones técnicas y alcance

La interfaz se implementa con React + Vite y se aloja en `Views/Home/Index.cshtml`. En el modo MVC, `HomeController`, `ProductsController` y `SalesController` atienden las rutas; `BodegaService` valida las operaciones y `Data/BodegaRepository.cs` ejecuta las transacciones contra SQL Server. El modo demo conserva `localStorage` para una demostración sin servidor, pero la implementación de Visual Studio usa MVC + SQL Server sin cambiar las entidades ni los flujos.
