using LogiMarket.Application.Comandos;
using LogiMarket.Application.Interfaces;
using LogiMarket.Application.Pedidos;
using LogiMarket.Application.Pedidos.Confirmacion;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Outbox;
using LogiMarket.Infrastructure.Repositories;
using LogiMarket.Tests.Soporte;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Tests.Aplicacion;

public class ConfirmacionTests : IDisposable
{
    private readonly Entorno _e = new();

    public void Dispose() => _e.Dispose();

    private Task<ResultadoConfirmacion> ConfirmarAsync(int pedidoId, string clave)
        => _e.EnAlcanceAsync(sp => sp.GetRequiredService<ConfirmacionPedidoFacade>().ConfirmarAsync(pedidoId, clave));

    [Fact]
    public async Task P02_AnaVipKitTeletrabajoEstandar()
    {
        var id = await _e.BorradorAsync("Ana", TipoEntrega.Estandar, ("KIT_TELETRABAJO", 1));
        _e.ComoRol(RolUsuario.Ventas);

        var r = await ConfirmarAsync(id, "ANA-001");

        Assert.True(r.Exito, r.Mensaje);
        Assert.Equal(59.00m, r.Total);
        Assert.Equal((7, 4, 19), (await _e.StockAsync("P01"), await _e.StockAsync("P02"), await _e.StockAsync("P03")));
        Assert.Equal("PAGADO", await _e.EstadoAsync(id));
        Assert.Equal(1, await _e.PagosAsync());
        Assert.Equal(1, await _e.EventosAsync(ConfirmacionPedidoFacade.EventoPedidoPagado));
        var historial = await _e.ConContextoAsync(ctx => ctx.PedidoHistorialEstados.Where(h => h.PedidoId == id)
            .OrderBy(h => h.Secuencia).Select(h => h.EstadoNuevo).ToListAsync());
        Assert.Equal(new[] { "BORRADOR", "VALIDADO", "PAGADO" }, historial);
    }

    [Fact]
    public async Task P03_LuisNormalWebcamYAlfombrillaExpres()
    {
        var id = await _e.BorradorAsync("Luis", TipoEntrega.Expres, ("P04", 2), ("P05", 2));
        _e.ComoRol(RolUsuario.Ventas);

        var r = await ConfirmarAsync(id, "LUIS-001");

        Assert.True(r.Exito, r.Mensaje);
        Assert.Equal(102.00m, r.Total);
        Assert.Equal((2, 28), (await _e.StockAsync("P04"), await _e.StockAsync("P05")));
        Assert.Equal(1, await _e.PagosAsync());
    }

    [Fact]
    public async Task P04_CarmenMayoristaDiezMemorias()
    {
        var id = await _e.BorradorAsync("Carmen", TipoEntrega.Estandar, ("P03", 10));
        _e.ComoRol(RolUsuario.Ventas);

        var r = await ConfirmarAsync(id, "CARMEN-001");

        Assert.True(r.Exito, r.Mensaje);
        Assert.Equal(90.00m, r.Total);
        Assert.Equal(10, await _e.StockAsync("P03"));
        var descuento = await _e.ConContextoAsync(ctx => ctx.Pedidos.Where(p => p.Id == id).Select(p => new { p.Descuento, p.NombreDescuento }).SingleAsync());
        Assert.Equal((15.00m, "MAYORISTA"), (descuento.Descuento, descuento.NombreDescuento));
    }

    [Fact]
    public async Task P05_SeisKitsTeletrabajoSeRechazanPorTeclados()
    {
        var id = await _e.BorradorAsync("Ana", TipoEntrega.Estandar, ("KIT_TELETRABAJO", 6));
        _e.ComoRol(RolUsuario.Ventas);

        var r = await ConfirmarAsync(id, "ANA-006");

        Assert.False(r.Exito);
        Assert.Equal("Stock disponible", r.Validacion!.EslabonFallido);
        Assert.Contains("P02 requiere 6 y hay 5", r.Mensaje);
        Assert.Equal((8, 5, 20), (await _e.StockAsync("P01"), await _e.StockAsync("P02"), await _e.StockAsync("P03")));
        Assert.Equal(0, await _e.PagosAsync());
        Assert.Equal(0, await _e.EventosAsync(ConfirmacionPedidoFacade.EventoPedidoPagado));
        Assert.Equal("BORRADOR", await _e.EstadoAsync(id));
        Assert.Empty(_e.MensajesPago());
    }

    [Fact]
    public async Task P06_CobroRechazadoNoDejaCambios()
    {
        var id = await _e.BorradorAsync("Luis", TipoEntrega.Expres, ("P04", 2));
        _e.ComoRol(RolUsuario.Ventas);
        _e.Control.FallarCobros = true;

        var r = await ConfirmarAsync(id, "LUIS-FALLA");

        Assert.False(r.Exito);
        Assert.Contains("TARJETA_RECHAZADA", r.Mensaje);
        Assert.Equal(4, await _e.StockAsync("P04"));
        Assert.Equal("BORRADOR", await _e.EstadoAsync(id));
        Assert.Equal(0, await _e.PagosAsync());
        Assert.Equal(0, await _e.EventosAsync(ConfirmacionPedidoFacade.EventoPedidoPagado));
        Assert.Equal(0, await _e.ConContextoAsync(ctx => ctx.MovimientosInventario.CountAsync(m => m.PedidoId == id)));
    }

    [Fact]
    public async Task P07_MismaClaveDevuelveElResultadoOriginalSinCobrarDosVeces()
    {
        var id = await _e.BorradorAsync("Ana", TipoEntrega.Estandar, ("KIT_TELETRABAJO", 1));
        _e.ComoRol(RolUsuario.Ventas);

        var primero = await ConfirmarAsync(id, "ANA-REPETIDA");
        var segundo = await ConfirmarAsync(id, "ANA-REPETIDA");

        Assert.True(primero.Exito);
        Assert.True(segundo.EsRepeticion);
        Assert.Equal(primero.ReferenciaCobro, segundo.ReferenciaCobro);
        Assert.Equal(1, await _e.PagosAsync());
        Assert.Equal(3, await _e.ConContextoAsync(ctx => ctx.MovimientosInventario.CountAsync(m => m.PedidoId == id)));
        Assert.Equal(7, await _e.StockAsync("P01"));
        Assert.Single(_e.MensajesPago(), m => m.StartsWith("INICIO"));
    }

    [Fact]
    public async Task P07_OtraClaveSobreUnPedidoPagadoTambienSeRechaza()
    {
        var id = await _e.BorradorAsync("Luis", TipoEntrega.Estandar, ("P05", 1));
        _e.ComoRol(RolUsuario.Ventas);
        await ConfirmarAsync(id, "CLAVE-A");

        var r = await ConfirmarAsync(id, "CLAVE-B");

        Assert.False(r.Exito);
        Assert.Equal(1, await _e.PagosAsync());
        Assert.Equal(29, await _e.StockAsync("P05"));
    }

    [Fact]
    public async Task P08_PedidoPagadoNoSeCancela()
    {
        var id = await _e.BorradorAsync("Luis", TipoEntrega.Estandar, ("P05", 1));
        _e.ComoRol(RolUsuario.Ventas);
        await ConfirmarAsync(id, "LUIS-P08");

        var r = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioPedidos>().CancelarAsync(id));

        Assert.False(r.Exito);
        Assert.Equal("PAGADO", await _e.EstadoAsync(id));
        Assert.Equal(3, await _e.ConContextoAsync(ctx => ctx.PedidoHistorialEstados.CountAsync(h => h.PedidoId == id)));
    }

    [Fact]
    public async Task P20_FalloDeUnColaboradorNoDejaPersistenciaParcial()
    {
        using var e = new Entorno(s => s.AddScoped<IEventoOutboxRepository, OutboxQueFalla>());
        var id = await e.BorradorAsync("Ana", TipoEntrega.Estandar, ("KIT_TELETRABAJO", 1));
        e.ComoRol(RolUsuario.Ventas);

        var r = await e.EnAlcanceAsync(sp => sp.GetRequiredService<ConfirmacionPedidoFacade>().ConfirmarAsync(id, "ANA-P20"));

        Assert.False(r.Exito);
        Assert.Equal(8, await e.StockAsync("P01"));
        Assert.Equal(0, await e.PagosAsync());
        Assert.Equal("BORRADOR", await e.EstadoAsync(id));
        Assert.Equal(0, await e.ConContextoAsync(ctx => ctx.MovimientosInventario.CountAsync(m => m.PedidoId == id)));
    }

    private sealed class OutboxQueFalla : IEventoOutboxRepository
    {
        public Task AgregarAsync(EventoOutbox evento) => throw new IOException("No se pudo escribir el evento (simulado).");
        public Task<EventoOutbox?> ObtenerPorIdAsync(int id) => Task.FromResult<EventoOutbox?>(null);
        public Task<IReadOnlyList<EventoOutbox>> ObtenerPendientesAsync(int maximo = 50) => Task.FromResult<IReadOnlyList<EventoOutbox>>(new List<EventoOutbox>());
        public Task<IReadOnlyList<EventoOutbox>> ListarAsync() => ObtenerPendientesAsync();
    }

    [Fact]
    public async Task P24_ElMismoComandoSirveParaBotonYPrueba()
    {
        var id = await _e.BorradorAsync("Ana", TipoEntrega.Estandar, ("KIT_TELETRABAJO", 1));
        _e.ComoRol(RolUsuario.Ventas);
        var invocador = _e.Proveedor.GetRequiredService<InvocadorComandos>();

        var resultado = await _e.EnAlcanceAsync(sp =>
            invocador.EjecutarAsync(new ConfirmarPedidoCommand(sp.GetRequiredService<ConfirmacionPedidoFacade>(), id, "ANA-CMD")));
        var repetido = await ConfirmarAsync(id, "ANA-CMD");

        Assert.True(resultado.Exito);
        Assert.True(repetido.EsRepeticion);
        var registro = Assert.Single(invocador.Historial);
        Assert.Equal(("ConfirmarPedido", "ventas", true), (registro.Comando, registro.Usuario, registro.Exito));
        Assert.Contains("clave=ANA-CMD", registro.Entrada);
    }

    [Fact]
    public async Task P36_DosCobrosConcurrentesSobreLaUltimaUnidad()
    {
        _e.ComoRol(RolUsuario.Admin);
        await _e.EnAlcanceAsync(async sp =>
        {
            var p04 = await sp.GetRequiredService<IProductoRepository>().ObtenerPorCodigoAsync("P04");
            p04!.DescontarStock(3);
            await sp.GetRequiredService<IUnitOfWork>().GuardarCambiosAsync();
            return 0;
        });
        var a = await _e.BorradorAsync("Luis", TipoEntrega.Estandar, ("P04", 1));
        var b = await _e.BorradorAsync("Ana", TipoEntrega.Estandar, ("P04", 1));
        _e.ComoRol(RolUsuario.Ventas);

        var resultados = await Task.WhenAll(
            Task.Run(() => ConfirmarAsync(a, "CONC-A")),
            Task.Run(() => ConfirmarAsync(b, "CONC-B")));

        Assert.Single(resultados, r => r.Exito);
        Assert.Equal(0, await _e.StockAsync("P04"));
        Assert.Equal(1, await _e.PagosAsync());
    }
}
