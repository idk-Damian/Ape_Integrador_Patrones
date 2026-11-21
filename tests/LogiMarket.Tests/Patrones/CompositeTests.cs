using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Exceptions;
using Xunit;

namespace LogiMarket.Tests.Patrones;

public class CompositeTests
{
    [Fact]
    public void KitCompuesto_CalculaPrecioRecursivamente_ConKitsAnidados()
    {
        // P01 (20) + P02 (30) = KIT_OFICINA (50)
        var p01 = new ProductoSimple("P01", "Mouse USB", 20.00m, 8, 0.10m);
        var p02 = new ProductoSimple("P02", "Teclado", 30.00m, 5, 0.60m);
        var p03 = new ProductoSimple("P03", "Memoria USB", 10.00m, 20, 0.30m);

        var kitOficina = new KitCompuesto("KIT_OFICINA", "Kit Oficina");
        kitOficina.AgregarComponente(p01, 1);
        kitOficina.AgregarComponente(p02, 1);

        var kitTeletrabajo = new KitCompuesto("KIT_TELETRABAJO", "Kit Teletrabajo");
        kitTeletrabajo.AgregarComponente(kitOficina, 1);
        kitTeletrabajo.AgregarComponente(p03, 1);

        Assert.Equal(50.00m, kitOficina.PrecioUnitario);
        Assert.Equal(60.00m, kitTeletrabajo.PrecioUnitario);
    }

    [Fact]
    public void KitCompuesto_ExpandeASkuFinales_ConCantidadesPropagadas()
    {
        var p01 = new ProductoSimple("P01", "Mouse USB", 20.00m, 8, 0.10m);
        var p02 = new ProductoSimple("P02", "Teclado", 30.00m, 5, 0.60m);
        var p03 = new ProductoSimple("P03", "Memoria USB", 10.00m, 20, 0.30m);

        var kitOficina = new KitCompuesto("KIT_OFICINA", "Kit Oficina");
        kitOficina.AgregarComponente(p01, 2); // 2 mouse
        kitOficina.AgregarComponente(p02, 1); // 1 teclado

        var kitTeletrabajo = new KitCompuesto("KIT_TELETRABAJO", "Kit Teletrabajo");
        kitTeletrabajo.AgregarComponente(kitOficina, 2); // 2 kits = 4 mouse, 2 teclado
        kitTeletrabajo.AgregarComponente(p03, 3); // 3 memorias

        var fisicos = kitTeletrabajo.ObtenerComponentesFisicos(1).ToList();

        Assert.Equal(3, fisicos.Count);
        Assert.Equal(4, fisicos.First(f => f.Codigo == "P01").Cantidad);
        Assert.Equal(2, fisicos.First(f => f.Codigo == "P02").Cantidad);
        Assert.Equal(3, fisicos.First(f => f.Codigo == "P03").Cantidad);
    }

    [Fact]
    public void KitCompuesto_BloqueaReferenciaCircularDirecta()
    {
        var kitA = new KitCompuesto("KIT_A", "Kit A");

        var ex = Assert.Throws<DomainException>(() =>
        {
            kitA.AgregarComponente(kitA, 1);
        });

        Assert.Contains("circular directa", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void KitCompuesto_BloqueaReferenciaCircularIndirecta()
    {
        var kitA = new KitCompuesto("KIT_A", "Kit A");
        var kitB = new KitCompuesto("KIT_B", "Kit B");

        kitA.AgregarComponente(kitB, 1);

        var ex = Assert.Throws<DomainException>(() =>
        {
            kitB.AgregarComponente(kitA, 1);
        });

        Assert.Contains("circular indirecta", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
