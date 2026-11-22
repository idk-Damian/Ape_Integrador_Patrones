using LogiMarket.Domain.Entregas;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Tests.Soporte;

namespace LogiMarket.Tests.Dominio;

public class MementoTests
{
    [Fact]
    public void P29_TresCambiosDosDeshacerYUnRehacer()
    {
        var d = new Datos();
        var calc = Datos.Calculadora();
        var pedido = d.Borrador();                          // P01 x1, estandar: 20 + 5 = 25

        pedido.AgregarLinea(d.KitTeletrabajo, 1, calc);     // 80 + 5 = 85
        pedido.ModificarCantidad("P01", 3, calc);           // 120 + 5 = 125
        pedido.CambiarEntrega(new FabricaEntregaExpres(), calc); // 120 + 12 = 132

        Assert.True(pedido.Deshacer(calc));
        Assert.True(pedido.Deshacer(calc));
        Assert.Equal(85.00m, pedido.Total);
        Assert.Equal(1, pedido.Lineas.Single(l => l.ElementoCodigo == "P01").Cantidad);

        Assert.True(pedido.Rehacer(calc));
        Assert.Equal(125.00m, pedido.Total);
        Assert.Equal(3, pedido.Lineas.Single(l => l.ElementoCodigo == "P01").Cantidad);
        Assert.Equal(2, pedido.Lineas.Count);
    }

    [Fact]
    public void P29_AlPagarSeBloqueaElHistorial()
    {
        var d = new Datos();
        var calc = Datos.Calculadora();
        var pedido = d.Borrador();
        pedido.AgregarLinea(d.P02, 1, calc);
        pedido.Validar(Datos.Hoy);
        pedido.Pagar(Datos.Hoy);

        Assert.False(pedido.PuedeDeshacer);
        Assert.Throws<DomainException>(() => pedido.Deshacer(calc));
        Assert.Equal(55.00m, pedido.Total);
    }

    [Fact]
    public void P29_SoloSeConservanCincoEstados()
    {
        var d = new Datos();
        var calc = Datos.Calculadora();
        var pedido = d.Borrador();
        for (int i = 2; i <= 7; i++) pedido.ModificarCantidad("P01", i, calc);

        int deshechos = 0;
        while (pedido.Deshacer(calc)) deshechos++;

        Assert.Equal(4, deshechos);
        Assert.Equal(3, pedido.Lineas[0].Cantidad);
    }
}
