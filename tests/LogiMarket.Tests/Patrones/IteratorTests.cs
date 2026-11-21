using LogiMarket.Domain.Catalogo;
using Xunit;

namespace LogiMarket.Tests.Patrones;

public class IteratorTests
{
    [Fact]
    public void IteradorComponentesFisicos_RecorreKitsAnidadosPropagandoCantidades()
    {
        var p01 = new ProductoSimple("P01", "Mouse USB", 20.00m, 8, 0.10m);
        var p02 = new ProductoSimple("P02", "Teclado", 30.00m, 5, 0.60m);
        var p03 = new ProductoSimple("P03", "Memoria USB", 10.00m, 20, 0.30m);

        var kitOficina = new KitCompuesto("KIT_OFICINA", "Kit Oficina");
        kitOficina.AgregarComponente(p01, 1);
        kitOficina.AgregarComponente(p02, 1);

        var kitTeletrabajo = new KitCompuesto("KIT_TELETRABAJO", "Kit Teletrabajo");
        kitTeletrabajo.AgregarComponente(kitOficina, 1);
        kitTeletrabajo.AgregarComponente(p03, 1);

        // Iterar con cantidad inicial = 2
        var iterador = new IteradorComponentesFisicos(kitTeletrabajo, 2);
        var lista = iterador.ToList();

        Assert.Equal(3, lista.Count);
        Assert.Equal(2, lista.First(c => c.Codigo == "P01").Cantidad);
        Assert.Equal(2, lista.First(c => c.Codigo == "P02").Cantidad);
        Assert.Equal(2, lista.First(c => c.Codigo == "P03").Cantidad);
    }
}
