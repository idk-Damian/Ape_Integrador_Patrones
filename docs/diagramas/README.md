# Diagramas — LogiMarket 360 (APE 04)

Fuente en Mermaid (`.mmd`) y exportación PNG a escala 2. Cada diagrama se armó leyendo el código del repositorio: referencias de los `.csproj`, `LogiMarketDbContextModelSnapshot.cs`, las clases de estado y los métodos reales de la fachada.

| Archivo | Diagrama | Fuente en el código |
|---|---|---|
| `01_contexto` | Contexto: 4 actores, el sistema, integraciones simuladas y almacenamiento local | `PoliticaPermisos`, `Infrastructure/Pagos`, `Notificadores`, `Program.cs` |
| `02_componentes` | Los 5 proyectos y sus referencias de compilación | `*.csproj` (Infrastructure solo se usa en `Program.cs`) |
| `03_modelo_relacional` | 15 tablas con PK, FK, índices únicos y CHECK | `Migrations/LogiMarketDbContextModelSnapshot.cs` |
| `04_estados_pedido` | Ciclo BORRADOR → VALIDADO → PAGADO → DESPACHADO → ENTREGADO y CANCELADO | `Domain/Pedidos/Estados/*` |
| `05_secuencia_confirmacion_aprobada` | Confirmación aprobada del caso de Ana (59.00), de la vista hasta el Mediator | `CobroView`, `ConfirmacionPedidoFacade.ConfirmarAsync`, `FormularioPrincipalMediator` |
| `06_secuencia_cobro_rechazado` | Operación rechazada: cobro rechazado por la pasarela, denegación en la cadena y en el Proxy | `ConfirmacionPedidoFacade`, `PagoAutorizadoProxy`, `AuditoriaPagoDecorator` |
| `07_clases_creacionales` | Factory Method, Abstract Factory, Builder, Prototype y Singleton | `Notificaciones`, `Entregas`, `PedidoBuilder`, `Pedido`, `MetricasSesion` |
| `08_clases_estructurales_a` | Adapter, Decorator, Proxy y Facade en la cadena de cobro | `Pagos`, `ConfirmacionPedidoFacade` |
| `08_clases_estructurales_b` | Bridge, Composite y Flyweight | `Comprobantes`, `Catalogo`, `Presentacion` |
| `09_clases_comportamentales_a` | Chain, Command, Interpreter, Iterator, Mediator y Memento | `Validacion`, `Comandos`, `Filtros`, `Catalogo`, `Presentacion`, `Memento` |
| `10_clases_comportamentales_b` | Observer, State, Strategy, Template Method y Visitor | `Eventos`, `Estados`, `Descuentos`, `Devoluciones`, `Catalogo` |

Para regenerar los PNG (requiere Node y `@mermaid-js/mermaid-cli`):

```bash
mmdc -i 04_estados_pedido.mmd -o 04_estados_pedido.png -s 2 -b white
```
