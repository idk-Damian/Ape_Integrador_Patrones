using LogiMarket.Domain.Catalogo;
using LogiMarket.Tests.Soporte;

namespace LogiMarket.Tests.Dominio;

public class IteratorTests
{
    [Fact]
    public void P27_RecorreKitTeletrabajoUnaVezPorSkuYTermina()
    {
        var d = new Datos();
        using var it = d.KitTeletrabajo.CrearIterador();
        var vistos = new List<ComponenteFisico>();
        while (it.MoveNext()) vistos.Add(it.Current);

        Assert.Equal(new[] { "P01", "P02", "P03" }, vistos.Select(v => v.Codigo));
        Assert.All(vistos, v => Assert.Equal(1, v.Cantidad));
        Assert.False(it.MoveNext());

        it.Reset();
        Assert.True(it.MoveNext());
        Assert.Equal("P01", it.Current.Codigo);
    }
}
