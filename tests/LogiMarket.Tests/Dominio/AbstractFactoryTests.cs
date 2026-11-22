using LogiMarket.Domain.Despachos;
using LogiMarket.Domain.Entregas;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Tests.Dominio;

public class AbstractFactoryTests
{
    [Fact]
    public void P12_CadaFamiliaEntregaTarifaPlazoYGuiaCoherentes()
    {
        var desde = new DateTime(2026, 11, 22);
        IFabricaEntrega estandar = new FabricaEntregaEstandar();
        IFabricaEntrega expres = new FabricaEntregaExpres();

        Assert.Equal((5.00m, 3), (estandar.CrearTarifa().Calcular(), estandar.CrearPlazo().Dias));
        Assert.Equal((12.00m, 1), (expres.CrearTarifa().Calcular(), expres.CrearPlazo().Dias));
        Assert.Equal(new DateTime(2026, 11, 25), estandar.CrearPlazo().EstimarEntrega(desde));
        Assert.StartsWith("STD-", estandar.CrearEtiqueta().GenerarNumeroGuia(7, desde));
        Assert.StartsWith("EXP-", expres.CrearEtiqueta().GenerarNumeroGuia(7, desde));
        Assert.Equal(TipoEntrega.Expres, expres.CrearTarifa().Familia);
        Assert.Equal(TipoEntrega.Expres, expres.CrearPlazo().Familia);
        Assert.Equal(TipoEntrega.Expres, expres.CrearEtiqueta().Familia);
    }

    [Fact]
    public void P12_NoSePuedeDespacharConUnaFamiliaDistintaALaVendida()
    {
        var pedido = new Soporte.Datos().Borrador();
        typeof(LogiMarket.Domain.Pedidos.Pedido).GetProperty("Id")!.SetValue(pedido, 10);

        Assert.Throws<DomainException>(() => Despacho.Registrar(pedido, new FabricaEntregaExpres(), DateTime.UtcNow));
        var despacho = Despacho.Registrar(pedido, new FabricaEntregaEstandar(), new DateTime(2026, 11, 22));
        Assert.Equal("STD-20261122-000010", despacho.NumeroGuia);
        Assert.Equal(new DateTime(2026, 11, 25), despacho.FechaEstimada);
    }
}
