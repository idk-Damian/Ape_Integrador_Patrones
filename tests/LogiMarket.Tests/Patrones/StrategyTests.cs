using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Pedidos.Descuentos;
using Xunit;

namespace LogiMarket.Tests.Patrones;

public class StrategyTests
{
    private readonly SelectorDescuento _selector = SelectorDescuento.CrearEstandar();

    [Fact]
    public void Mayorista15_AplicaCuandoUnidadesMayorOIgualA10_PrioridadSobreVip()
    {
        // 10 unidades físicas finales en cliente VIP -> Aplica MAYORISTA (15%), no VIP (10%)
        var estrategia = _selector.Seleccionar(10, CategoriaCliente.Vip);

        Assert.Equal("MAYORISTA", estrategia.Nombre);
        Assert.Equal(15.00m, estrategia.CalcularDescuento(100.00m));
    }

    [Fact]
    public void Vip10_AplicaCuandoClienteVipYUnidadesMenorA10()
    {
        var estrategia = _selector.Seleccionar(5, CategoriaCliente.Vip);

        Assert.Equal("VIP", estrategia.Nombre);
        Assert.Equal(10.00m, estrategia.CalcularDescuento(100.00m));
    }

    [Fact]
    public void SinDescuento_AplicaParaClienteNormalConMenosDe10Unidades()
    {
        var estrategia = _selector.Seleccionar(5, CategoriaCliente.Normal);

        Assert.Equal("NORMAL", estrategia.Nombre);
        Assert.Equal(0.00m, estrategia.CalcularDescuento(100.00m));
    }

    [Fact]
    public void RedondeoMoneda_AwayFromZero_Correcto()
    {
        var mayorista = new Mayorista15();
        // 33.33 * 0.15 = 4.9995 -> 5.00
        Assert.Equal(5.00m, mayorista.CalcularDescuento(33.33m));
    }
}
