using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Clientes;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.Pedidos;
using Xunit;

namespace LogiMarket.Tests.Patrones;

public class BuilderTests
{
    [Fact]
    public void PedidoBuilder_ConstruyePedidoExitosamente()
    {
        var cliente = new Cliente("Ana López", "ana@example.com", CategoriaCliente.Vip);
        var p01 = new ProductoSimple("P01", "Mouse USB", 20.00m, 8, 0.20m);

        var pedido = new PedidoBuilder()
            .ConCliente(cliente)
            .AgregarProducto(p01, 2)
            .ConTipoEntrega(TipoEntrega.EnvioEstandar)
            .ConTarifaEnvio(5.00m)
            .Construir();

        Assert.Equal(cliente.Id, pedido.ClienteId);
        Assert.Equal(40.00m, pedido.Subtotal);
        Assert.Equal(4.00m, pedido.Descuento); // 10% VIP (2 unidades < 10)
        Assert.Equal(5.00m, pedido.TarifaEnvio);
        Assert.Equal(41.00m, pedido.Total); // 40 - 4 + 5 = 41
    }

    [Fact]
    public void PedidoBuilder_BloqueaSinCliente()
    {
        var p01 = new ProductoSimple("P01", "Mouse USB", 20.00m, 8, 0.20m);

        var builder = new PedidoBuilder()
            .AgregarProducto(p01, 1);

        var ex = Assert.Throws<DomainException>(() => builder.Construir());
        Assert.Contains("sin cliente", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PedidoBuilder_BloqueaSinLineas()
    {
        var cliente = new Cliente("Ana López", "ana@example.com", CategoriaCliente.Vip);

        var builder = new PedidoBuilder()
            .ConCliente(cliente);

        var ex = Assert.Throws<DomainException>(() => builder.Construir());
        Assert.Contains("sin líneas", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
