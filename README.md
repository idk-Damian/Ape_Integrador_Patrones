# LogiMarket 360

Aplicación de escritorio (Windows Forms, .NET 8) para la venta y la logística de accesorios de computación. Cubre catálogo con kits anidados, pedidos, cobro simulado, despacho, devoluciones, auditoría y consultas. Está hecha con Clean Architecture, SOLID y los 23 patrones GoF.

## Requisitos

- Windows 10 u 11.
- SDK de .NET 8 o superior (`dotnet --list-sdks`). Visual Studio 2022 o posterior con la carga de trabajo de escritorio .NET, si se quiere abrir la solución en el IDE.
- Opcional, solo para generar migraciones nuevas: la herramienta `dotnet-ef` (`dotnet tool install --global dotnet-ef`).

No se necesita un servidor de base de datos: se usa SQLite en un archivo local.

## Estructura

| Proyecto | Referencias | Contenido |
|---|---|---|
| `src/LogiMarket.Domain` | ninguna | Entidades, invariantes, estados, kits, descuentos, familias de entrega |
| `src/LogiMarket.Application` | Domain | Casos de uso, puertos, validación, autorización, fachada, eventos, comprobantes, filtros |
| `src/LogiMarket.Infrastructure` | Application, Domain | EF Core + SQLite, repositorios, migraciones, semilla, pasarelas simuladas, notificadores, bitácora |
| `src/LogiMarket.WinForms` | Application (Infrastructure solo en `Program.cs`) | Pantallas, composition root de DI |
| `tests/LogiMarket.Tests` | Domain, Application, Infrastructure | Pruebas unitarias y de integración (xUnit) |

## Instalación y compilación

```bash
dotnet restore LogiMarket360.sln
dotnet build LogiMarket360.sln
```

## Base de datos, migraciones y semilla

Al iniciar, la aplicación crea `datos/logimarket.db` junto al ejecutable. Aplica las migraciones de EF Core y carga la semilla: 5 productos, 2 kits, 3 clientes y 4 usuarios. La semilla es idempotente, así que se puede ejecutar varias veces sin duplicar datos.

Para aplicar las migraciones manualmente sobre una base de diseño:

```bash
dotnet ef database update --project src/LogiMarket.Infrastructure --startup-project src/LogiMarket.Infrastructure
```

Para crear una migración nueva después de cambiar el modelo:

```bash
dotnet ef migrations add NombreDeLaMigracion --project src/LogiMarket.Infrastructure --startup-project src/LogiMarket.Infrastructure
```

## Ejecución

```bash
dotnet run --project src/LogiMarket.WinForms
```

En Visual Studio se puede marcar `LogiMarket.WinForms` como proyecto de inicio y pulsar F5.

### Cuentas de demostración

Son datos ficticios, solo para pruebas locales.

| Usuario | Contraseña | Rol | Puede |
|---|---|---|---|
| admin | Admin123! | ADMIN | todo |
| ventas | Ventas123! | VENTAS | clientes, pedidos, validación, cobro, devoluciones |
| bodega | Bodega123! | BODEGA | despacho y entrega |
| consulta | Consulta123! | CONSULTA | solo lectura |

Las contraseñas se guardan con PBKDF2 (SHA-256, sal aleatoria). Los permisos se verifican en Application: un botón habilitado no salta la regla.

## Pruebas automatizadas

```bash
dotnet test LogiMarket360.sln
```

Las pruebas de integración crean una base SQLite temporal para cada caso. Aplican las migraciones reales y la semilla, y borran el archivo al terminar. Para obtener un reporte TRX:

```bash
dotnet test LogiMarket360.sln --logger "trx;LogFileName=pruebas.trx" --results-directory resultados
```

## Restablecer los datos de prueba

- **Desde la aplicación:** inicie sesión como `admin`, abra **Auditoría y demostración**, pestaña **Demostración**, y pulse **Restablecer base de pruebas**. La aplicación pide confirmación y deja la base como recién sembrada.
- **Manualmente:** con la aplicación cerrada, borre la carpeta `datos` que está junto al ejecutable (`src/LogiMarket.WinForms/bin/Debug/net8.0-windows/datos`). Se volverá a crear al iniciar.

## Salidas generadas

Todo queda dentro de `datos/`, junto al ejecutable:

- `logs/*.log`: bitácora técnica por categoría (`pago`, `seguridad`, `notificacion`, `outbox`, `comando`, `negocio`).
- `comprobantes/*.txt`: comprobantes internos de venta y devolución. Son de demostración y no tienen valor tributario.
- `errores.log`: errores inesperados, si los hubiera.

## Integraciones simuladas

El cobro, el reembolso, el correo y el SMS son simulados y no salen del equipo. Desde **Auditoría y demostración** se puede:

- cambiar el proveedor de pago (legado o alternativo);
- inducir rechazos de cobro;
- inducir fallos del notificador y reprocesar el Outbox;
- comparar la memoria con y sin Flyweight.

## Patrones GoF y dónde están

| Patrón | Clase principal |
|---|---|
| Factory Method | `Application/Notificaciones/CreadorNotificador`, `Infrastructure/Notificaciones/Notificadores` |
| Abstract Factory | `Domain/Entregas/IFabricaEntrega`, `FabricaEntregaEstandar`, `FabricaEntregaExpres` |
| Builder | `Domain/Pedidos/PedidoBuilder` |
| Prototype | `Domain/Pedidos/Pedido.ClonarComoBorrador` |
| Singleton | `Application/Metricas/MetricasSesion` (único en DI) |
| Adapter | `Infrastructure/Pagos/PasarelaLegadaAdapter` sobre `Pagos/Legado/PasarelaLegadaSimulada` |
| Bridge | `Application/Comprobantes/Comprobantes` (`Comprobante` × `ISalidaDocumento`) |
| Composite | `Domain/Catalogo/ProductoSimple`, `KitCompuesto` |
| Decorator | `Infrastructure/Pagos/AuditoriaPagoDecorator` |
| Facade | `Application/Pedidos/Confirmacion/ConfirmacionPedidoFacade` |
| Flyweight | `Application/Presentacion/CatalogoVisualFlyweightFactory` |
| Proxy | `Application/Pagos/PagoAutorizadoProxy` |
| Chain of Responsibility | `Application/Pedidos/Validacion` |
| Command | `Application/Comandos` |
| Interpreter | `Application/Consultas/Filtros` |
| Iterator | `Domain/Catalogo/IteradorComponentesFisicos` |
| Mediator | `Application/Presentacion/FormularioPrincipalMediator` y las vistas de WinForms |
| Memento | `Domain/Pedidos/Memento` |
| Observer | `Application/Eventos` (publicador, observadores y procesador del Outbox) |
| State | `Domain/Pedidos/Estados` |
| Strategy | `Domain/Pedidos/Descuentos` |
| Template Method | `Application/Devoluciones/ProcesadorDevolucionBase` |
| Visitor | `Domain/Catalogo/VisitanteCatalogoRecursivo` y sus visitantes |
