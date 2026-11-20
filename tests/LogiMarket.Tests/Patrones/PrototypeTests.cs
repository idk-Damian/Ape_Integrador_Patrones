using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.Pedidos;
using Xunit;

namespace LogiMarket.Tests.Patrones;

public class PrototypeTests
{
    [Fact]
    public void ClonarComoBorrador_CopiaProfunda_ModificarClonNoAlteraOriginal()
    {
        var p01 = new ProductoSimple("P01", "Mouse USB", 20.00m, 8, 0.10m);
        var p02 = new ProductoSimple("P02", "Teclado", 30.00m, 5, 0.60m);

        var pedidoOriginal = new PedidoBuilder()
            .ConCliente(1, CategoriaCliente.Normal)
            .AgregarProducto(p01, 1)
            .Construir();

        var clon = pedidoOriginal.ClonarComoBorrador();

        // Modificar el clon agregando otra línea
        clon.AgregarLinea(new LineaPedido(p02, 1));

        Assert.Single(pedidoOriginal.Lineas);
        Assert.Equal(2, clon.Lineas.Count);
        Assert.Equal(20.00m, pedidoOriginal.Subtotal);
        Assert.Equal(50.00m, clon.Subtotal);
    }

    [Fact]
    public void ClonarComoBorrador_SoloPermitidoEnBorrador()
    {
        var p01 = new ProductoSimple("P01", "Mouse USB", 20.00m, 8, 0.10m);
        var pedido = new PedidoBuilder()
            .ConCliente(1, CategoriaCliente.Normal)
            .AgregarProducto(p01, 1)
            .Construir();

        pedido.Validar();

        var ex = Assert.Throws<DomainException>(() => pedido.ClonarComoBorrador());
        Assert.Contains("BORRADOR", ex.Message);
    }
}
