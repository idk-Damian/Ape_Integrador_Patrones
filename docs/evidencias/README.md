# Evidencias técnicas — LogiMarket 360 (APE 04)

Todo lo que hay en esta carpeta salió de ejecuciones reales sobre el código del repositorio. Nada se editó a mano, salvo los documentos `.md`, que resumen esos resultados.

## Pruebas automatizadas

| Archivo | Contenido |
|---|---|
| `pruebas_consola.txt` | Salida de `dotnet test` con verbosidad normal: 117 pruebas, 117 correctas (22/11/2026 21:29, commit `694601d`) |
| `pruebas.trx` | El mismo resultado en formato TRX (se abre con Visual Studio) |
| `matriz_P01_P38.md` | Matriz de los 38 casos de la guía: entrada, esperado, obtenido, evidencia y archivo/método |

## Logs pedidos por la guía

| Archivo | Caso | Qué muestra |
|---|---|---|
| `P19_decorator.txt` | P19 | Secuencia INICIO → PASARELA → FIN/ERROR con correlación, en un cobro aprobado, uno rechazado y uno con excepción |
| `P22_proxy.txt` | P22 | Denegación a CONSULTA y BODEGA sin ningún evento de PAGO (no llega al Decorator ni al Adapter), frente a VENTAS autorizado |
| `P37_outbox.txt` | P37 | Evento pendiente tras la caída del notificador, recuperación con un procesador reiniciado y segunda pasada sin duplicados |
| `P21_flyweight.txt` | P21 | Tamaño, número de instancias, condiciones de medición y comparación con el mismo escenario sin Flyweight |

Estos cuatro archivos los generan las pruebas de `tests/LogiMarket.Tests/Evidencias/EvidenciasTests.cs` cuando existe la variable `LOGIMARKET_EVIDENCIAS`:

```powershell
$env:LOGIMARKET_EVIDENCIAS = "$PWD\docs\evidencias"
dotnet test LogiMarket360.sln
```

## Riesgos (Actividad 7)

| Archivo | Contenido |
|---|---|
| `riesgos_actividad7.md` | Tabla de los riesgos R01–R10 y R14 (y R11–R13 sin prueba roja), con principio, refactorización, commits y prueba antes/después |
| `riesgos/R01-R09_antes.txt` | Pruebas sobre el commit `5efa8ee`: 34 pruebas, 8 con error |
| `riesgos/R01-R09_despues.txt` | Pruebas sobre el commit `2b7a824`: 46 de 46 |
| `riesgos/R10_antes.txt` | Pruebas sobre el commit `b4bfbdb`: 110 pruebas, 2 con error |
| `riesgos/R10_despues.txt` | Pruebas después de `c6a54ff`: 116 de 116 |
| `riesgos/R14_antes.txt` | Pruebas sobre el commit `322fe7c`: 117 pruebas, 1 con error |
| `riesgos/R14_despues.txt` | Pruebas sobre el commit `694601d`: 117 de 117 |

## Verificación operativa (Actividad 9)

| Archivo | Contenido |
|---|---|
| `fase1/fase1_verificacion.txt` | Clon limpio, restore, build sin advertencias, pruebas y dos arranques de la app desde una base inexistente |
| `fase1/fase1_pruebas.txt` | Salida de las pruebas en el clon limpio (108 de 108 en ese momento) |
| `fase1/fase1_persistencia.txt` | Cobro del pedido de Ana desde la interfaz, cierre total y reapertura con los mismos datos |
| `fase1/fase1_fusion.txt` | Fusión `--no-ff` de la fase 1 en `develop` y `main`, y push |

## Correspondencia de hashes

El 22/11/2026 se normalizaron los mensajes de los commits posteriores a `ef51d35` para quitar líneas de metadatos que no describían el cambio. El contenido de cada commit no cambió: los árboles antes y después son idénticos en las cuatro ramas publicadas. Los logs crudos de `fase1/` y `riesgos/` se generaron antes de la normalización y muestran los hashes anteriores; esta tabla da su equivalencia.

| Antes | Ahora | Commit |
|---|---|---|
| `22b2937` | `5efa8ee` | test: reproduce defectos detectados en la fase 1 |
| `2cbe5f1` | `2b7a824` | refactor: corrige invariantes de la fase 1 y segrega puertos de persistencia |
| `cff0688` | `f8d1a63` | feat(pagos): sesion por rol y cadena de cobro Proxy -> Decorator -> Adapter |
| `8fb6df2` | `04a65f0` | feat(pedidos): cadena de validacion, fachada de confirmacion, logistica y comandos |
| `7cce112` | `8e9edf6` | feat(eventos): outbox con observer, notificaciones, devoluciones, comprobantes y filtros |
| `d360c53` | `fbd2094` | feat: interfaz WinForms con 12 pantallas, mediator y flyweight |
| `dd6ef93` | `2f14bbc` | fix(ui): escalado por DPI, ventana maximizada y textos completos |
| `7ac9be9` | `7719bcf` | merge: integra fase 1 en develop |
| `ce0ed0b` | `c229471` | merge: publica en main la version de la fase 1 |
| `8824d45` | `b4bfbdb` | test: reproduce R10, el pedido ignora el reloj inyectado |
| `b702149` | `c6a54ff` | fix: el pedido recibe la fecha del reloj inyectado (R10) |
| `fc7dfa6` | `1ff885f` | test: evidencias legibles para P19, P22, P37 y P21 |

Los commits `d6a8f26`, `bd4f512`, `61abc67` (PR #1) y `ef51d35` no cambiaron.
