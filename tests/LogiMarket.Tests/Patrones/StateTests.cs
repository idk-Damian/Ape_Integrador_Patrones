using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.Pedidos;
using Xunit;

namespace LogiMarket.Tests.Patrones;

public class StateTests
{
    private Pedido CrearPedidoBorrador()
    {
        var p01 = new ProductoSimple("P01", "Mouse USB", 20.00m, 8, 0.20m);
        return new PedidoBuilder()
            .ConCliente(1, CategoriaCliente.Normal)
            .AgregarProducto(p01, 1)
            .Construir();
    }

    [Fact]
    public void FlujoCompleto_TransicionesValidas()
    {
        var pedido = CrearPedidoBorrador();
        Assert.Equal("BORRADOR", pedido.EstadoNombre);
        Assert.True(pedido.PuedeEditar);

        pedido.Validar();
        Assert.Equal("VALIDADO", pedido.EstadoNombre);
        Assert.False(pedido.PuedeEditar);

        pedido.Pagar();
        Assert.Equal("PAGADO", pedido.EstadoNombre);

        pedido.Despachar();
        Assert.Equal("DESPACHADO", pedido.EstadoNombre);

        pedido.Entregar();
        Assert.Equal("ENTREGADO", pedido.EstadoNombre);
    }

    [Fact]
    public void Cancelar_PermitidoDesdeBorradorYValidado()
    {
        var p1 = CrearPedidoBorrador();
        p1.Cancelar();
        Assert.Equal("CANCELADO", p1.EstadoNombre);

        var p2 = CrearPedidoBorrador();
        p2.Validar();
        p2.Cancelar();
        Assert.Equal("CANCELADO", p2.EstadoNombre);
    }

    [Fact]
    public void Pagado_ProhibidoCancelarOEditar()
    {
        var pedido = CrearPedidoBorrador();
        pedido.Validar();
        pedido.Pagar();

        Assert.Throws<DomainException>(() => pedido.Cancelar());
        Assert.False(pedido.PuedeEditar);

        var p02 = new ProductoSimple("P02", "Teclado", 30.00m, 5, 0.70m);
        Assert.Throws<DomainException>(() => pedido.AgregarLinea(new LineaPedido(p02, 1)));
    }

    [Fact]
    public void Despachar_AntesDePagar_LanzaExcepcion()
    {
        var pedido = CrearPedidoBorrador();
        pedido.Validar();

        Assert.Throws<DomainException>(() => pedido.Despachar());
    }

    [Fact]
    public void RetrocederEstados_Prohibido()
    {
        var pedido = CrearPedidoBorrador();
        pedido.Validar();
        pedido.Pagar();
        pedido.Despachar();

        Assert.Throws<DomainException>(() => pedido.Validar());
        Assert.Throws<DomainException>(() => pedido.Pagar());
    }
}
