using LogiMarket.Domain.Entregas;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Tests.Soporte;

namespace LogiMarket.Tests.Dominio;

public class StateTests
{
    [Fact]
    public void P31_CicloValidoCompleto()
    {
        var pedido = new Datos().Borrador();

        pedido.Validar(Datos.Hoy);
        pedido.Pagar(Datos.Hoy);
        pedido.Despachar(Datos.Hoy);
        pedido.Entregar(new DateTime(2026, 11, 1));

        Assert.Equal("ENTREGADO", pedido.EstadoNombre);
        Assert.Equal(new DateTime(2026, 11, 1), pedido.FechaEntrega);
    }

    [Fact]
    public void P31_CancelarSoloDesdeBorradorOValidado()
    {
        var d = new Datos();
        var a = d.Borrador();
        a.Cancelar(Datos.Hoy);
        var b = d.Borrador();
        b.Validar(Datos.Hoy);
        b.Cancelar(Datos.Hoy);

        Assert.Equal("CANCELADO", a.EstadoNombre);
        Assert.Equal("CANCELADO", b.EstadoNombre);
    }

    [Fact]
    public void P31_PagadoNoSeCancelaNiSeEdita()
    {
        var d = new Datos();
        var pedido = d.Borrador();
        pedido.Validar(Datos.Hoy);
        pedido.Pagar(Datos.Hoy);

        Assert.Throws<DomainException>(() => pedido.Cancelar(Datos.Hoy));
        Assert.Throws<DomainException>(() => pedido.AgregarLinea(d.P02, 1, Datos.Calculadora()));
        Assert.Throws<DomainException>(() => pedido.ModificarCantidad("P01", 5, Datos.Calculadora()));
        Assert.Equal("PAGADO", pedido.EstadoNombre);
        Assert.Equal(25.00m, pedido.Total);
    }

    [Fact]
    public void P09_NoSeDespachaAntesDePagar()
    {
        var pedido = new Datos().Borrador();
        pedido.Validar(Datos.Hoy);

        Assert.Throws<DomainException>(() => pedido.Despachar(Datos.Hoy));
        Assert.Equal("VALIDADO", pedido.EstadoNombre);
    }

    [Fact]
    public void P31_NoSeRetrocedeNiSeRepiteEstado()
    {
        var pedido = new Datos().Borrador();
        pedido.Validar(Datos.Hoy);
        pedido.Pagar(Datos.Hoy);

        Assert.Throws<DomainException>(() => pedido.Validar(Datos.Hoy));
        Assert.Throws<DomainException>(() => pedido.Pagar(Datos.Hoy));
    }

    [Fact]
    public void P31_CadaTransicionQuedaEnElHistorialDeEstados()
    {
        var pedido = new Datos().Borrador();
        pedido.Validar(Datos.Hoy);
        pedido.Pagar(Datos.Hoy);
        pedido.Despachar(Datos.Hoy);

        Assert.Equal(new[] { "BORRADOR", "VALIDADO", "PAGADO", "DESPACHADO" }, pedido.CambiosEstado.Select(c => c.EstadoNuevo));
        Assert.Null(pedido.CambiosEstado[0].EstadoAnterior);
        Assert.Equal("PAGADO", pedido.CambiosEstado[3].EstadoAnterior);
    }
}
