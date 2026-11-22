using LogiMarket.Application.Comprobantes;
using LogiMarket.Application.Devoluciones;
using LogiMarket.Application.Eventos;
using LogiMarket.Domain.Enums;
using LogiMarket.Tests.Soporte;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Tests.Aplicacion;

public class DevolucionTests : IDisposable
{
    private readonly Entorno _e = new();

    public void Dispose() => _e.Dispose();

    private async Task<ResultadoDevolucion> DevolverAsync(int pedidoId, MotivoDevolucion motivo, bool sinUso = true)
    {
        ResultadoDevolucion? r = null;
        await _e.ComoAsync(RolUsuario.Ventas, async () =>
            r = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioDevoluciones>()
                .ProcesarAsync(new SolicitudDevolucion(pedidoId, motivo, sinUso))));
        return r!;
    }

    [Fact]
    public async Task P35_AnaDevuelvePorDefectoYRecibe54SinFlete()
    {
        var id = await _e.EntregadoAsync("Ana", TipoEntrega.Estandar, ("KIT_TELETRABAJO", 1));
        _e.Reloj.Avanzar(TimeSpan.FromDays(5));

        var r = await DevolverAsync(id, MotivoDevolucion.Defecto);
        var repetida = await DevolverAsync(id, MotivoDevolucion.Defecto);

        Assert.True(r.Aceptada, r.Mensaje);
        Assert.Equal(54.00m, r.Reembolso);
        Assert.Equal(new[] { "1. Verificar elegibilidad", "2. Inspeccionar condición", "3. Calcular reembolso",
            "4. Registrar devolución, stock y reembolso", "5. Generar evento" }, r.Pasos);
        Assert.False(repetida.Aceptada);
        Assert.Equal((8, 5, 20), (await _e.StockAsync("P01"), await _e.StockAsync("P02"), await _e.StockAsync("P03")));
        Assert.Equal(1, await _e.ConContextoAsync(ctx => ctx.Devoluciones.CountAsync()));
        Assert.Equal(3, await _e.ConContextoAsync(ctx => ctx.MovimientosInventario.CountAsync(m => m.Tipo == TipoMovimiento.ReingresoDevolucion)));
        Assert.Single(_e.MensajesPago(), m => m.StartsWith("REEMBOLSO 54.00") || m.StartsWith("REEMBOLSO 54,00"));
        Assert.Equal(1, await _e.EventosAsync(TiposEvento.DevolucionAceptada));
    }

    [Fact]
    public async Task P33_DefectoAlDia20SeAcepta()
    {
        var id = await _e.EntregadoAsync("Luis", TipoEntrega.Expres, ("P04", 2), ("P05", 2));
        _e.Reloj.Avanzar(TimeSpan.FromDays(20));

        var r = await DevolverAsync(id, MotivoDevolucion.Defecto, sinUso: false);

        Assert.True(r.Aceptada, r.Mensaje);
        Assert.Equal(20, r.DiasDesdeEntrega);
        Assert.Equal(90.00m, r.Reembolso);
        Assert.Equal((4, 30), (await _e.StockAsync("P04"), await _e.StockAsync("P05")));
    }

    [Fact]
    public async Task P33_ArrepentimientoAlDia10SeRechazaSinTocarStockNiReembolsar()
    {
        var id = await _e.EntregadoAsync("Luis", TipoEntrega.Expres, ("P04", 2));
        _e.Reloj.Avanzar(TimeSpan.FromDays(10));

        var r = await DevolverAsync(id, MotivoDevolucion.Arrepentimiento);

        Assert.False(r.Aceptada);
        Assert.Contains("7 días", r.Mensaje);
        Assert.Equal(new[] { "1. Verificar elegibilidad", "2. Inspeccionar condición" }, r.Pasos);
        Assert.Equal(2, await _e.StockAsync("P04"));
        Assert.Equal(0, await _e.ConContextoAsync(ctx => ctx.Devoluciones.CountAsync()));
        Assert.DoesNotContain(_e.MensajesPago(), m => m.StartsWith("REEMBOLSO"));
    }

    [Fact]
    public async Task ArrepentimientoConProductoUsadoSeRechaza()
    {
        var id = await _e.EntregadoAsync("Luis", TipoEntrega.Estandar, ("P05", 1));
        _e.Reloj.Avanzar(TimeSpan.FromDays(3));

        var r = await DevolverAsync(id, MotivoDevolucion.Arrepentimiento, sinUso: false);

        Assert.False(r.Aceptada);
        Assert.Contains("sin uso", r.Mensaje);
    }

    [Fact]
    public async Task SoloSeDevuelvenPedidosEntregados()
    {
        var id = await _e.PagadoAsync("Luis", TipoEntrega.Estandar, ("P05", 1));

        var r = await DevolverAsync(id, MotivoDevolucion.Defecto);

        Assert.False(r.Aceptada);
        Assert.Contains("ENTREGADOS", r.Mensaje);
        Assert.Equal(29, await _e.StockAsync("P05"));
    }

    [Theory]
    [InlineData(MediosSalida.VistaPrevia)]
    [InlineData(MediosSalida.ArchivoTxt)]
    public async Task P17_DosComprobantesPorDosMedios(string medio)
    {
        var id = await _e.EntregadoAsync("Ana", TipoEntrega.Estandar, ("KIT_TELETRABAJO", 1));
        await DevolverAsync(id, MotivoDevolucion.Defecto);

        ResultadoSalida? venta = null, devolucion = null;
        await _e.ComoAsync(RolUsuario.Consulta, async () =>
        {
            venta = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioComprobantes>().EmitirVentaAsync(id, medio));
            devolucion = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioComprobantes>().EmitirDevolucionAsync(id, medio));
        });

        Assert.Contains("COMPROBANTE DE VENTA", venta!.Contenido);
        Assert.Contains("59.00", venta.Contenido.Replace(',', '.'));
        Assert.Contains("COMPROBANTE DE DEVOLUCIÓN", devolucion!.Contenido);
        Assert.Contains("54.00", devolucion.Contenido.Replace(',', '.'));

        if (medio == MediosSalida.ArchivoTxt)
        {
            Assert.True(File.Exists(venta.Ruta));
            Assert.True(File.Exists(devolucion.Ruta));
            Assert.Equal(venta.Contenido, File.ReadAllText(venta.Ruta!));
        }
        else
        {
            Assert.Null(venta.Ruta);
        }
    }
}
