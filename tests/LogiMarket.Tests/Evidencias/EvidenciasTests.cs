using System.Globalization;
using System.Text;
using LogiMarket.Application;
using LogiMarket.Application.Comun;
using LogiMarket.Application.Eventos;
using LogiMarket.Application.Excepciones;
using LogiMarket.Application.Pagos;
using LogiMarket.Application.Pedidos.Confirmacion;
using LogiMarket.Application.Presentacion;
using LogiMarket.Domain.Enums;
using LogiMarket.Infrastructure;
using LogiMarket.Infrastructure.Comun;
using LogiMarket.Infrastructure.Data;
using LogiMarket.Infrastructure.Pagos;
using LogiMarket.Tests.Soporte;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace LogiMarket.Tests.Evidencias;

// escenarios que la guia pide documentar con logs (P19, P22, P37 y P21).
// Cada prueba verifica con Assert y ademas deja el texto legible; si existe la variable
// LOGIMARKET_EVIDENCIAS, el texto se guarda en esa carpeta para el informe
public class EvidenciasTests : IDisposable
{
    private readonly Entorno _e = new();
    private readonly ITestOutputHelper _salida;

    public EvidenciasTests(ITestOutputHelper salida) => _salida = salida;

    public void Dispose() => _e.Dispose();

    [Fact]
    public async Task P19_SecuenciaDeLogsDelDecoratorEnLaCadenaReal()
    {
        var texto = new List<string> { "P19 - Decorator de auditoria sobre la cadena real Proxy -> Decorator -> Adapter (DI de la aplicacion)", "" };

        var aprobado = await _e.PagadoAsync("Ana", TipoEntrega.Estandar, ("KIT_TELETRABAJO", 1));
        texto.Add($"1) Cobro aprobado: pedido #{aprobado} de Ana, proveedor {_e.Control.ProveedorPago}");
        texto.AddRange(Lineas(_e.Bitacora.Entradas(CategoriasBitacora.Pago)));

        var antes = _e.Bitacora.Entradas(CategoriasBitacora.Pago).Count;
        var rechazado = await _e.BorradorAsync("Luis", TipoEntrega.Expres, ("P04", 2), ("P05", 2));
        _e.Control.FallarCobros = true;
        ResultadoConfirmacion? r = null;
        await _e.ComoAsync(RolUsuario.Ventas, async () =>
            r = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ConfirmacionPedidoFacade>().ConfirmarAsync(rechazado, "P19-RECHAZO")));
        _e.Control.FallarCobros = false;
        var rechazo = _e.Bitacora.Entradas(CategoriasBitacora.Pago).Skip(antes).ToList();

        texto.Add("");
        texto.Add($"2) Cobro rechazado por la pasarela: pedido #{rechazado} de Luis -> {r!.Mensaje}");
        texto.AddRange(Lineas(rechazo));

        // error tecnico del proveedor: el decorador anota ERROR y deja pasar la excepcion
        var bitacora = new BitacoraTecnica(_e.Reloj);
        var caido = new AuditoriaPagoDecorator(new ProveedorSinRespuesta(), bitacora);
        await Assert.ThrowsAsync<TimeoutException>(() => caido.CobrarAsync(15m, "P19-ERROR"));
        texto.Add("");
        texto.Add("3) Proveedor sin respuesta (excepcion): el decorador registra ERROR y no la oculta");
        texto.AddRange(Lineas(bitacora.Entradas(CategoriasBitacora.Pago)));

        texto.Add("");
        texto.Add($"Estado final: pedido #{aprobado} {await _e.EstadoAsync(aprobado)}, pedido #{rechazado} {await _e.EstadoAsync(rechazado)}, pagos en base {await _e.PagosAsync()}");

        Guardar("P19_decorator.txt", texto);

        var aprobadoLog = _e.Bitacora.Entradas(CategoriasBitacora.Pago).Take(antes).Select(Primera).ToList();
        Assert.Equal(new[] { "INICIO", "PASARELA", "FIN" }, aprobadoLog);
        Assert.Equal(new[] { "INICIO", "PASARELA", "FIN" }, rechazo.Select(Primera));
        Assert.Contains("RECHAZADO", rechazo[2].Mensaje);
        Assert.Single(rechazo.Select(e => e.Correlacion).Where(c => c != null).Distinct());
        Assert.False(r.Exito);
        Assert.Equal("BORRADOR", await _e.EstadoAsync(rechazado));
        Assert.Equal(1, await _e.PagosAsync());
    }

    [Fact]
    public async Task P22_DenegacionDelProxyNoLlegaAlDecoratorNiAlProveedor()
    {
        var texto = new List<string> { "P22 - Proxy de proteccion delante del cobro (cadena real resuelta por DI)", "" };
        var stockP02 = await _e.StockAsync("P02");

        foreach (var rol in new[] { RolUsuario.Consulta, RolUsuario.Bodega })
        {
            var pagosAntes = _e.Bitacora.Entradas(CategoriasBitacora.Pago).Count;
            var seguridadAntes = _e.Bitacora.Entradas(CategoriasBitacora.Seguridad).Count;
            Exception? error = null;

            await _e.ComoAsync(rol, async () =>
            {
                using var alcance = _e.Proveedor.CreateScope();
                var pasarela = alcance.ServiceProvider.GetRequiredService<IPasarelaPago>();
                error = await Record.ExceptionAsync(() => pasarela.CobrarAsync(59m, $"P22-{rol}"));
            });

            var pagosNuevos = _e.Bitacora.Entradas(CategoriasBitacora.Pago).Skip(pagosAntes).ToList();
            var seguridadNuevos = _e.Bitacora.Entradas(CategoriasBitacora.Seguridad).Skip(seguridadAntes).ToList();
            texto.Add($"Rol {rol.ToString().ToUpperInvariant()}: {error?.GetType().Name} - {error?.Message}");
            texto.Add($"  diferencia de eventos -> PAGO +{pagosNuevos.Count} (INICIO/PASARELA/FIN), SEGURIDAD +{seguridadNuevos.Count}");
            texto.AddRange(Lineas(seguridadNuevos).Select(l => "  " + l));

            Assert.IsType<AccesoDenegadoException>(error);
            Assert.Empty(pagosNuevos);
            Assert.Single(seguridadNuevos);
        }

        var pedido = await _e.BorradorAsync("Ana", TipoEntrega.Estandar, ("KIT_TELETRABAJO", 1));
        ResultadoConfirmacion? viaFachada = null;
        await _e.ComoAsync(RolUsuario.Consulta, async () =>
            viaFachada = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ConfirmacionPedidoFacade>().ConfirmarAsync(pedido, "P22-FACHADA")));
        texto.Add("");
        texto.Add($"Confirmacion por la fachada con CONSULTA: {viaFachada!.Mensaje}");
        texto.Add($"  pagos en base {await _e.PagosAsync()}, stock P02 {stockP02} -> {await _e.StockAsync("P02")}, estado {await _e.EstadoAsync(pedido)}, eventos outbox {await _e.ConContextoAsync(c => c.EventosOutbox.CountAsync())}");

        var antesVentas = _e.Bitacora.Entradas(CategoriasBitacora.Pago).Count;
        await _e.ComoAsync(RolUsuario.Ventas, async () =>
        {
            using var alcance = _e.Proveedor.CreateScope();
            var cobro = await alcance.ServiceProvider.GetRequiredService<IPasarelaPago>().CobrarAsync(59m, "P22-VENTAS");
            Assert.True(cobro.Aprobado);
        });
        var ventas = _e.Bitacora.Entradas(CategoriasBitacora.Pago).Skip(antesVentas).ToList();
        texto.Add("");
        texto.Add($"Contraste con VENTAS (autorizado): PAGO +{ventas.Count}");
        texto.AddRange(Lineas(ventas).Select(l => "  " + l));

        Guardar("P22_proxy.txt", texto);

        Assert.False(viaFachada.Exito);
        Assert.Equal(0, await _e.PagosAsync());
        Assert.Equal(stockP02, await _e.StockAsync("P02"));
        Assert.Equal(new[] { "INICIO", "PASARELA", "FIN" }, ventas.Select(Primera));
    }

    [Fact]
    public async Task P37_OutboxFallaYSeRecuperaTrasReiniciarElProcesador()
    {
        var texto = new List<string> { "P37 - Outbox: falla el notificador y el evento se recupera al reiniciar el procesador", "" };

        var pedido = await _e.PagadoAsync("Ana", TipoEntrega.Estandar, ("KIT_TELETRABAJO", 1));
        texto.Add($"Pedido #{pedido} pagado; la transaccion dejo el evento en EventosOutbox.");
        texto.Add("  " + await FilaOutboxAsync(_e.Proveedor));

        _e.Control.FallarNotificaciones = true;
        var fallido = await _e.ProcesarOutboxAsync();
        texto.Add("");
        texto.Add($"1) Procesador con el notificador caido: procesados {fallido.Procesados}, fallidos {fallido.Fallidos}");
        texto.AddRange(fallido.Detalle.Select(d => "  " + d));
        texto.Add("  " + await FilaOutboxAsync(_e.Proveedor));
        texto.Add($"  auditorias de negocio {await _e.ConContextoAsync(c => c.Auditorias.CountAsync())}, avisos enviados {_e.Bitacora.Entradas(CategoriasBitacora.Notificacion).Count}");

        // reinicio: otro contenedor de servicios sobre el mismo archivo SQLite, sin estado en memoria del anterior
        var servicios = new ServiceCollection();
        servicios.AgregarInfraestructura($"Data Source={_e.RutaBase};Pooling=False", Path.Combine(_e.CarpetaSalidas, "reinicio"));
        servicios.AgregarAplicacion();
        servicios.AddSingleton<IReloj>(_e.Reloj);
        await using var reiniciado = servicios.BuildServiceProvider();
        _e.Reloj.Avanzar(TimeSpan.FromMinutes(5));

        ResultadoOutbox recuperado, repetido;
        using (var alcance = reiniciado.CreateScope())
            recuperado = await alcance.ServiceProvider.GetRequiredService<ProcesadorOutbox>().ProcesarPendientesAsync();
        using (var alcance = reiniciado.CreateScope())
            repetido = await alcance.ServiceProvider.GetRequiredService<ProcesadorOutbox>().ProcesarPendientesAsync();

        var bitacora = reiniciado.GetRequiredService<IBitacora>();
        texto.Add("");
        texto.Add($"2) Procesador reiniciado (nuevo ServiceProvider): procesados {recuperado.Procesados}, fallidos {recuperado.Fallidos}");
        texto.AddRange(recuperado.Detalle.Select(d => "  " + d));
        texto.Add("  " + await FilaOutboxAsync(reiniciado));
        texto.AddRange(Lineas(bitacora.Entradas(CategoriasBitacora.Notificacion)).Select(l => "  " + l));
        texto.Add("");
        texto.Add($"3) Segunda pasada: procesados {repetido.Procesados}, fallidos {repetido.Fallidos} (no se duplica)");
        texto.Add($"  pagos en base {await _e.PagosAsync()}, auditorias de negocio {await _e.ConContextoAsync(c => c.Auditorias.CountAsync())}, avisos enviados {bitacora.Entradas(CategoriasBitacora.Notificacion).Count}");
        texto.Add("");
        texto.Add("Bitacora OUTBOX del procesador original y del reiniciado:");
        texto.AddRange(Lineas(_e.Bitacora.Entradas(CategoriasBitacora.Outbox).Concat(bitacora.Entradas(CategoriasBitacora.Outbox))).Select(l => "  " + l));

        Guardar("P37_outbox.txt", texto);

        var final = await _e.ConContextoAsync(c => c.EventosOutbox.SingleAsync());
        Assert.Equal((0, 1), (fallido.Procesados, fallido.Fallidos));
        Assert.Equal((1, 0), (recuperado.Procesados, recuperado.Fallidos));
        Assert.Equal(0, repetido.Procesados);
        Assert.NotNull(final.FechaProcesado);
        Assert.Equal(2, final.Intentos);
        Assert.Single(bitacora.Entradas(CategoriasBitacora.Notificacion));
        Assert.Equal(1, await _e.PagosAsync());
        Assert.Equal(1, await _e.ConContextoAsync(c => c.Auditorias.CountAsync()));
    }

    [Fact]
    public async Task P21_FlyweightComparadoConUnEscenarioEquivalente()
    {
        var origen = await _e.ConContextoAsync(async c => (await c.Productos.AsNoTracking().OrderBy(p => p.Codigo).ToListAsync())
            .Select(p => new OrigenFila(p.Codigo, p.Nombre, p.Categoria, p.Descripcion ?? "", p.Stock, p.PrecioUnitario)).ToList());

        var texto = new List<string>
        {
            "P21 - Flyweight: metadatos de SKU compartidos frente a un escenario equivalente sin Flyweight",
            "",
            "Condiciones de medicion:",
            $"  runtime {Environment.Version}, {System.Runtime.InteropServices.RuntimeInformation.OSDescription.Trim()}, proceso {(Environment.Is64BitProcess ? "x64" : "x86")}, configuracion {Configuracion}",
            $"  {origen.Count} SKU de la semilla, mismas filas y mismos datos en ambos escenarios, mismo hilo",
            "  GC.Collect antes de cada escenario y calentamiento previo de 10 filas; se mide GC.GetAllocatedBytesForCurrentThread",
            "  cada tamano se mide 5 veces; se informa la mediana",
            "",
            "Filas   | Metadatos con FW | Metadatos sin FW | Bytes con FW | Bytes sin FW | Ahorro | ReferenceEquals"
        };

        foreach (var filas in new[] { 1000, 10000 })
        {
            var mediciones = Enumerable.Range(0, 5).Select(_ => ComparativaFlyweight.Medir(origen, filas).Medicion).ToList();
            var m = mediciones.OrderBy(x => x.BytesConFlyweight).ElementAt(2);
            var sin = mediciones.Select(x => x.BytesSinFlyweight).OrderBy(x => x).ElementAt(2);
            var ahorro = 1 - (double)m.BytesConFlyweight / sin;
            texto.Add($"{filas,-7} | {m.ObjetosMetadatosConFlyweight,16} | {m.ObjetosMetadatosSinFlyweight,16} | {m.BytesConFlyweight,12:N0} | {sin,12:N0} | {ahorro.ToString("P1", CultureInfo.InvariantCulture),6} | {m.MismaReferencia}");

            Assert.Equal(origen.Count, m.ObjetosMetadatosConFlyweight);
            Assert.Equal(filas, m.ObjetosMetadatosSinFlyweight);
            Assert.True(m.MismaReferencia);
            Assert.True(m.BytesConFlyweight < sin);
        }

        texto.Add("");
        texto.Add("Lectura: con Flyweight solo existen tantos objetos de metadatos como SKU distintos; sin Flyweight hay uno por fila.");
        texto.Add("Stock, cantidad y precio de venta se guardan por fila (estado extrinseco) en ambos escenarios.");
        Guardar("P21_flyweight.txt", texto);
    }

    private sealed class ProveedorSinRespuesta : IPasarelaPago
    {
        public string Proveedor => "SIN_RESPUESTA";
        public Task<ResultadoCobro> CobrarAsync(decimal monto, string claveIdempotencia) => throw new TimeoutException("sin respuesta del banco");
    }

#if DEBUG
    private const string Configuracion = "Debug";
#else
    private const string Configuracion = "Release";
#endif

    private static string Primera(EntradaBitacora e) => e.Mensaje.Split(' ')[0];

    private static IEnumerable<string> Lineas(IEnumerable<EntradaBitacora> entradas)
        => entradas.Select(e => $"{e.Fecha:yyyy-MM-dd HH:mm:ss} [{e.Categoria}] {(e.Correlacion is null ? "" : $"<{e.Correlacion}> ")}{e.Mensaje}");

    private static async Task<string> FilaOutboxAsync(IServiceProvider proveedor)
    {
        using var alcance = proveedor.CreateScope();
        var e = await alcance.ServiceProvider.GetRequiredService<LogiMarketDbContext>().EventosOutbox.AsNoTracking().SingleAsync();
        return $"EventosOutbox #{e.Id} {e.TipoEvento}: intentos {e.Intentos}, procesado {(e.FechaProcesado is null ? "NO" : $"{e.FechaProcesado:yyyy-MM-dd HH:mm:ss}")}, error '{e.UltimoError ?? ""}'";
    }

    private void Guardar(string archivo, List<string> lineas)
    {
        var contenido = string.Join(Environment.NewLine, lineas);
        _salida.WriteLine(contenido);
        var carpeta = Environment.GetEnvironmentVariable("LOGIMARKET_EVIDENCIAS");
        if (string.IsNullOrWhiteSpace(carpeta)) return;
        Directory.CreateDirectory(carpeta);
        File.WriteAllText(Path.Combine(carpeta, archivo), contenido + Environment.NewLine, new UTF8Encoding(true));
    }
}
