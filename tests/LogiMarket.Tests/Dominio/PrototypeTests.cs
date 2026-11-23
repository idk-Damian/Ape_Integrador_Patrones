using LogiMarket.Domain.Entregas;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Tests.Soporte;

namespace LogiMarket.Tests.Dominio;

public class PrototypeTests
{
    [Fact]
    public void P14_LaCopiaEsProfundaYNoAfectaALaPlantilla()
    {
        var d = new Datos();
        var plantilla = Datos.Builder().ParaCliente(1, CategoriaCliente.Normal)
            .ConLinea(d.KitTeletrabajo, 1).ConEntrega(new FabricaEntregaEstandar()).Construir();

        var copia1 = plantilla.ClonarComoBorrador(Datos.Hoy);
        var copia2 = plantilla.ClonarComoBorrador(Datos.Hoy);
        copia1.ModificarCantidad("KIT_TELETRABAJO", 4, Datos.Calculadora());

        Assert.Equal(1, plantilla.Lineas[0].Cantidad);
        Assert.Equal(1, copia2.Lineas[0].Cantidad);
        Assert.Equal(4, copia1.Lineas[0].Cantidad);
        Assert.Equal(240.00m, copia1.Subtotal);
        Assert.NotSame(plantilla.Lineas[0], copia2.Lineas[0]);
        Assert.NotSame(plantilla.Lineas[0].Componentes[0], copia2.Lineas[0].Componentes[0]);
        Assert.Equal(0, copia2.Id);
        Assert.Equal("BORRADOR", copia2.EstadoNombre);
    }

    [Fact]
    public void P14_SoloUnBorradorSirveDePlantilla()
    {
        var pedido = new Datos().Borrador();
        pedido.Validar(Datos.Hoy);
        Assert.Throws<DomainException>(() => pedido.ClonarComoBorrador(Datos.Hoy));
    }
}
