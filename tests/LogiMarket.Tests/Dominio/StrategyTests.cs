using LogiMarket.Domain.Common;
using LogiMarket.Domain.Entregas;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.Pedidos.Descuentos;
using LogiMarket.Tests.Soporte;

namespace LogiMarket.Tests.Dominio;

public class StrategyTests
{
    [Fact]
    public void P32_AnaVipRecibeDiezPorCiento()
    {
        var r = Datos.Calculadora().Calcular(new ContextoDescuento(3, CategoriaCliente.Vip, 60.00m));
        Assert.Equal(("VIP", 6.00m), (r.Politica, r.Monto));
    }

    [Fact]
    public void P32_CarmenConDiezUnidadesRecibeMayorista()
    {
        var r = Datos.Calculadora().Calcular(new ContextoDescuento(10, CategoriaCliente.Normal, 100.00m));
        Assert.Equal(("MAYORISTA", 15.00m), (r.Politica, r.Monto));
    }

    [Fact]
    public void P32_SiCoincidenVipYMayoristaGanaMayoristaSinAcumular()
    {
        var r = Datos.Calculadora().Calcular(new ContextoDescuento(12, CategoriaCliente.Vip, 200.00m));
        Assert.Equal(("MAYORISTA", 30.00m), (r.Politica, r.Monto));
    }

    [Fact]
    public void P32_ClienteNormalSinVolumenNoTieneDescuento()
    {
        var r = Datos.Calculadora().Calcular(new ContextoDescuento(4, CategoriaCliente.Normal, 90.00m));
        Assert.Equal(("NORMAL", 0.00m), (r.Politica, r.Monto));
    }

    private sealed class Temporada20 : IEstrategiaDescuento
    {
        public string Nombre => "TEMPORADA";
        public int Prioridad => 200;
        public bool Aplica(ContextoDescuento c) => c.Subtotal >= 500m;
        public decimal CalcularDescuento(decimal subtotal) => Moneda.Redondear(subtotal * 0.20m);
    }

    [Fact]
    public void P32_NuevaPromocionSeAgregaSinEditarLaCalculadora()
    {
        var calc = new CalculadoraDescuento(new IEstrategiaDescuento[]
            { new Mayorista15(), new Vip10(), new SinDescuento(), new Temporada20() });

        var r = calc.Calcular(new ContextoDescuento(1, CategoriaCliente.Vip, 600.00m));
        Assert.Equal(("TEMPORADA", 120.00m), (r.Politica, r.Monto));
    }

    [Fact]
    public void P32_PrioridadesRepetidasSeRechazan()
    {
        Assert.Throws<DomainException>(() => new CalculadoraDescuento(new IEstrategiaDescuento[] { new SinDescuento(), new SinDescuento() }));
    }

    [Fact]
    public void RedondeoMonetarioAlejaDeCero()
    {
        Assert.Equal(0.13m, Moneda.Redondear(0.125m));
        Assert.Equal(1.01m, Moneda.Redondear(1.005m));
    }

    [Fact]
    public void CasosOficiales_TotalesDeAnaLuisYCarmen()
    {
        var d = new Datos();
        var ana = Datos.Builder().ParaCliente(1, CategoriaCliente.Vip).ConLinea(d.KitTeletrabajo, 1)
            .ConEntrega(new FabricaEntregaEstandar()).Construir();
        var luis = Datos.Builder().ParaCliente(2, CategoriaCliente.Normal).ConLinea(d.P04, 2).ConLinea(d.P05, 2)
            .ConEntrega(new FabricaEntregaExpres()).Construir();
        var carmen = Datos.Builder().ParaCliente(3, CategoriaCliente.Normal).ConLinea(d.P03, 10)
            .ConEntrega(new FabricaEntregaEstandar()).Construir();

        Assert.Equal((60.00m, 6.00m, 5.00m, 59.00m), (ana.Subtotal, ana.Descuento, ana.TarifaEnvio, ana.Total));
        Assert.Equal((90.00m, 0.00m, 12.00m, 102.00m), (luis.Subtotal, luis.Descuento, luis.TarifaEnvio, luis.Total));
        Assert.Equal((100.00m, 15.00m, 5.00m, 90.00m), (carmen.Subtotal, carmen.Descuento, carmen.TarifaEnvio, carmen.Total));
        Assert.Equal(54.00m, ana.ImporteProductos);
    }
}
