using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Tests.Soporte;

namespace LogiMarket.Tests.Dominio;

public class CompositeTests
{
    [Fact]
    public void P18_PreciosDeKitsSeCalculanRecursivamente()
    {
        var d = new Datos();
        Assert.Equal(50.00m, d.KitOficina.PrecioUnitario);
        Assert.Equal(60.00m, d.KitTeletrabajo.PrecioUnitario);
    }

    [Fact]
    public void P18_KitAnidadoSeExpandeHastaLosSkuFinales()
    {
        var d = new Datos();
        var fisicos = d.KitTeletrabajo.ObtenerComponentesFisicos(2).ToList();

        Assert.Equal(new[] { "P01", "P02", "P03" }, fisicos.Select(f => f.Codigo));
        Assert.All(fisicos, f => Assert.Equal(2, f.Cantidad));
    }

    [Fact]
    public void P18_KitNoPuedeContenerseDirectamente()
    {
        var d = new Datos();
        Assert.Throws<DomainException>(() => d.KitOficina.AgregarComponente(d.KitOficina, 1));
    }

    [Fact]
    public void P18_KitNoPuedeContenerseIndirectamente()
    {
        var d = new Datos();
        var ex = Assert.Throws<DomainException>(() => d.KitOficina.AgregarComponente(d.KitTeletrabajo, 1));
        Assert.Contains("indirecta", ex.Message);
    }
}
