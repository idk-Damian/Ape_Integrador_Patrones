# Evidencias técnicas — LogiMarket 360 (APE 04)

Todo lo que hay en esta carpeta salió de ejecuciones reales sobre el código del repositorio. Nada se editó a mano, salvo los documentos `.md`, que resumen esos resultados.

## Pruebas automatizadas

| Archivo | Contenido |
|---|---|
| `pruebas_consola.txt` | Salida de `dotnet test` con verbosidad normal: 116 pruebas, 116 correctas (22/11/2026 15:07) |
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
| `riesgos_actividad7.md` | Tabla de los riesgos R01–R10 (y R11–R13 sin prueba roja), con principio, refactorización, commits y prueba antes/después |
| `riesgos/R01-R09_antes.txt` | Pruebas sobre el commit `22b2937`: 34 pruebas, 8 con error |
| `riesgos/R01-R09_despues.txt` | Pruebas sobre el commit `2cbe5f1`: 46 de 46 |
| `riesgos/R10_antes.txt` | Pruebas sobre el commit `8824d45`: 110 pruebas, 2 con error |
| `riesgos/R10_despues.txt` | Pruebas después de `b702149`: 116 de 116 |

## Verificación operativa (Actividad 9)

| Archivo | Contenido |
|---|---|
| `fase1/fase1_verificacion.txt` | Clon limpio, restore, build sin advertencias, pruebas y dos arranques de la app desde una base inexistente |
| `fase1/fase1_pruebas.txt` | Salida de las pruebas en el clon limpio (108 de 108 en ese momento) |
| `fase1/fase1_persistencia.txt` | Cobro del pedido de Ana desde la interfaz, cierre total y reapertura con los mismos datos |
| `fase1/fase1_fusion.txt` | Fusión `--no-ff` de la fase 1 en `develop` y `main`, y push |
