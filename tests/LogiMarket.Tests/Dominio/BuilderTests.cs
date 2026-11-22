using LogiMarket.Domain.Entregas;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Tests.Soporte;

namespace LogiMarket.Tests.Dominio;

public class BuilderTests
{
    [Fact]
    public void P13_PedidoDeVariosRenglonesConKitYProducto()
    {
        var d = new Datos();
        var pedido = Datos.Builder()
            .ParaCliente(1, CategoriaCliente.Normal)
            .ConLinea(d.KitTeletrabajo, 1)
            .ConLinea(d.P05, 2)
            .ConLinea(d.P05, 1)
            .ConEntrega(new FabricaEntregaExpres())
            .Construir();

        Assert.Equal(2, pedido.Lineas.Count);
        Assert.Equal(3, pedido.Lineas.Single(l => l.ElementoCodigo == "P05").Cantidad);
        Assert.Equal(3, pedido.Lineas.Single(l => l.EsKit).Componentes.Count);
        Assert.Equal("BORRADOR", pedido.EstadoNombre);
        Assert.Equal(TipoEntrega.Expres, pedido.TipoEntrega);
    }

    [Fact]
    public void P13_SinClienteNoSeConstruye()
    {
        var d = new Datos();
        var b = Datos.Builder().ConLinea(d.P01, 1).ConEntrega(new FabricaEntregaEstandar());
        Assert.Throws<DomainException>(() => b.Construir());
    }

    [Fact]
    public void P13_SinLineasNoSeConstruye()
    {
        var b = Datos.Builder().ParaCliente(1, CategoriaCliente.Normal).ConEntrega(new FabricaEntregaEstandar());
        Assert.Throws<DomainException>(() => b.Construir());
    }

    [Fact]
    public void P13_LineaConCantidadCeroSeRechaza()
    {
        var d = new Datos();
        Assert.Throws<DomainException>(() => Datos.Builder().ConLinea(d.P01, 0));
    }
}
