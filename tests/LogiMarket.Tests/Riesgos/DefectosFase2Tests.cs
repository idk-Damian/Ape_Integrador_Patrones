using LogiMarket.Application.Consultas;
using LogiMarket.Application.Logistica;
using LogiMarket.Application.Pedidos;
using LogiMarket.Application.Pedidos.Confirmacion;
using LogiMarket.Domain.Enums;
using LogiMarket.Tests.Soporte;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Tests.Riesgos;

// R10: se vio en la pantalla de pedidos una hora distinta a la de auditoria y outbox.
// El pedido tomaba la hora UTC del sistema en vez del reloj inyectado (IReloj).
public class DefectosFase2Tests : IDisposable
{
    private readonly Entorno _e = new();

    public void Dispose() => _e.Dispose();

    [Fact]
    public async Task R10_PedidoDelCarritoDebeUsarElRelojInyectado()
    {
        var hora = _e.Reloj.Ahora;
        var id = 0;
        await _e.ComoAsync(RolUsuario.Ventas, async () =>
        {
            var carrito = _e.Proveedor.GetRequiredService<ServicioCarrito>();
            await carrito.SeleccionarClienteAsync(await _e.ClienteAsync("Ana"));
            await carrito.AgregarAsync("KIT_TELETRABAJO", 1);
            id = await carrito.GuardarAsync();
            var cobro = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ConfirmacionPedidoFacade>().ConfirmarAsync(id, "R10-CARRITO"));
            Assert.True(cobro.Exito, cobro.Mensaje);
        });

        var detalle = await DetalleAsync(id);

        Assert.Equal(hora, detalle.Resumen.Fecha);
        Assert.Equal(new[] { "BORRADOR", "VALIDADO", "PAGADO" }, detalle.Historial.Select(c => c.Hacia));
        Assert.All(detalle.Historial, c => Assert.Equal(hora, c.Fecha));
    }

    [Fact]
    public async Task R10_DespachoYEntregaDebenQuedarConLaHoraDelReloj()
    {
        var id = await _e.PagadoAsync("Ana", TipoEntrega.Estandar, ("P01", 1));

        _e.Reloj.Avanzar(TimeSpan.FromDays(1));
        var horaDespacho = _e.Reloj.Ahora;
        await _e.ComoAsync(RolUsuario.Bodega, async () =>
            Assert.True((await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioLogistica>().DespacharAsync(id))).Exito));

        _e.Reloj.Avanzar(TimeSpan.FromDays(2));
        var horaEntrega = _e.Reloj.Ahora;
        await _e.ComoAsync(RolUsuario.Bodega, async () =>
            Assert.True((await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioLogistica>().EntregarAsync(id))).Exito));

        var historial = (await DetalleAsync(id)).Historial;

        Assert.Equal("DESPACHADO", historial[3].Hacia);
        Assert.Equal(horaDespacho, historial[3].Fecha);
        Assert.Equal("ENTREGADO", historial[4].Hacia);
        Assert.Equal(horaEntrega, historial[4].Fecha);
    }

    private async Task<DetallePedido> DetalleAsync(int id)
    {
        DetallePedido? detalle = null;
        await _e.ComoAsync(RolUsuario.Consulta, async () =>
            detalle = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioConsultas>().DetalleAsync(id)));
        return detalle!;
    }
}
