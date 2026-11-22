using LogiMarket.Domain.Catalogo;
using LogiMarket.Tests.Soporte;

namespace LogiMarket.Tests.Dominio;

public class VisitorTests
{
    [Fact]
    public void P34_KitTeletrabajoPesaUnKiloYSeDesglosaEnTresSku()
    {
        var d = new Datos();
        var peso = new VisitantePesoLogistico();
        var desglose = new VisitanteDesgloseFisico();

        d.KitTeletrabajo.Aceptar(peso);
        d.KitTeletrabajo.Aceptar(desglose);

        Assert.Equal(1.00m, peso.PesoTotalKg);
        Assert.Equal(new[] { "P01", "P02", "P03" }, desglose.Lineas.Select(l => l.Codigo));
    }

    [Fact]
    public void P34_LaVisitaBajaPorElKitAnidado()
    {
        var d = new Datos();
        var peso = new VisitantePesoLogistico(3);

        d.KitTeletrabajo.Aceptar(peso);

        Assert.Equal(new[] { "KIT_TELETRABAJO", "KIT_OFICINA" }, peso.KitsVisitados);
        Assert.Equal(3.00m, peso.PesoTotalKg);
    }
}
