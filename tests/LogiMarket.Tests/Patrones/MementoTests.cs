using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.Pedidos;
using Xunit;

namespace LogiMarket.Tests.Patrones;

public class MementoTests
{
    [Fact]
    public void HistorialPedido_SoportaUndoYRedo()
    {
        var p01 = new ProductoSimple("P01", "Mouse USB", 20.00m, 8, 0.20m);
        var p02 = new ProductoSimple("P02", "Teclado", 30.00m, 5, 0.70m);

        var pedido = new PedidoBuilder()
            .ConCliente(1, CategoriaCliente.Normal)
            .AgregarProducto(p01, 1)
            .Construir();

        Assert.Equal(20.00m, pedido.Subtotal);

        // Edición 1
        pedido.AgregarLinea(new LineaPedido(p02, 1));
        Assert.Equal(50.00m, pedido.Subtotal);

        // Deshacer edición 1
        Assert.True(pedido.PuedeDeshacer);
        pedido.Deshacer();
        Assert.Equal(20.00m, pedido.Subtotal);
        Assert.Single(pedido.Lineas);

        // Rehacer
        Assert.True(pedido.PuedeRehacer);
        pedido.Rehacer();
        Assert.Equal(50.00m, pedido.Subtotal);
        Assert.Equal(2, pedido.Lineas.Count);
    }

    [Fact]
    public void HistorialPedido_SeBloqueaAlConfirmarPago()
    {
        var p01 = new ProductoSimple("P01", "Mouse USB", 20.00m, 8, 0.20m);
        var pedido = new PedidoBuilder()
            .ConCliente(1, CategoriaCliente.Normal)
            .AgregarProducto(p01, 1)
            .Construir();

        pedido.Validar();
        pedido.Pagar();

        Assert.False(pedido.PuedeDeshacer);
        Assert.Throws<DomainException>(() => pedido.Deshacer());
    }
}
