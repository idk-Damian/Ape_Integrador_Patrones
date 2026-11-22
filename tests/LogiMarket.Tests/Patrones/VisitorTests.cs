using LogiMarket.Domain.Catalogo;
using Xunit;

namespace LogiMarket.Tests.Patrones;

public class VisitorTests
{
    [Fact]
    public void VisitantePesoLogistico_KitTeletrabajo_SumaExactamenteUnKilogramo()
    {
        // P01 (0.20) + P02 (0.70) + P03 (0.10) = exactamente 1.00 kg
        var p01 = new ProductoSimple("P01", "Mouse USB", 20.00m, 8, 0.20m);
        var p02 = new ProductoSimple("P02", "Teclado", 30.00m, 5, 0.70m);
        var p03 = new ProductoSimple("P03", "Memoria USB", 10.00m, 20, 0.10m);

        var kitOficina = new KitCompuesto("KIT_OFICINA", "Kit Oficina");
        kitOficina.AgregarComponente(p01, 1);
        kitOficina.AgregarComponente(p02, 1);

        var kitTeletrabajo = new KitCompuesto("KIT_TELETRABAJO", "Kit Teletrabajo");
        kitTeletrabajo.AgregarComponente(kitOficina, 1);
        kitTeletrabajo.AgregarComponente(p03, 1);

        var visitante = new VisitantePesoLogistico(1);
        kitTeletrabajo.Aceptar(visitante);

        Assert.Equal(1.00m, visitante.PesoTotal);
    }

    [Fact]
    public void VisitanteDesgloseFisico_AgrupaCantidadesPorSku()
    {
        var p01 = new ProductoSimple("P01", "Mouse USB", 20.00m, 8, 0.20m);
        var p02 = new ProductoSimple("P02", "Teclado", 30.00m, 5, 0.70m);

        var kitOficina = new KitCompuesto("KIT_OFICINA", "Kit Oficina");
        kitOficina.AgregarComponente(p01, 2);
        kitOficina.AgregarComponente(p02, 1);

        var visitante = new VisitanteDesgloseFisico(3); // 3 kits = 6 P01, 3 P02
        kitOficina.Aceptar(visitante);

        Assert.Equal(6, visitante.Desglose["P01"].Cantidad);
        Assert.Equal(3, visitante.Desglose["P02"].Cantidad);
    }
}
