using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Entregas;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.Pedidos;
using LogiMarket.Domain.Pedidos.Descuentos;
using LogiMarket.Infrastructure.Data;
using LogiMarket.Infrastructure.Repositories;
using LogiMarket.Tests.Soporte;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Tests.Riesgos;

// mismas pruebas del commit "test: reproduce defectos detectados en la fase 1",
// adaptadas a la API corregida; antes fallaban, ahora deben pasar
public class DefectosFase1Tests : IDisposable
{
    private readonly BaseDePruebas _bd = new();

    public void Dispose() => _bd.Dispose();

    [Fact]
    public void R01_Iterador_NoDebeReportarCicloSiUnKitApareceEnDosRamas()
    {
        var d = new Datos();
        var rama = new KitCompuesto("KIT_RAMA", "Rama");
        rama.AgregarComponente(d.KitOficina, 1);
        rama.AgregarComponente(d.P03, 1);
        var raiz = new KitCompuesto("KIT_RAIZ", "Raiz");
        raiz.AgregarComponente(d.KitOficina, 1);
        raiz.AgregarComponente(rama, 1);

        using var it = raiz.CrearIterador();
        int unidades = 0;
        while (it.MoveNext()) unidades += it.Current.Cantidad;

        Assert.Equal(5, unidades);
        Assert.Equal(110.00m, raiz.PrecioUnitario);
    }

    [Fact]
    public void R03_EstrategiaElegidaNoDebePerderseAlEditarElBorrador()
    {
        var d = new Datos();
        var soloNormal = new CalculadoraDescuento(new IEstrategiaDescuento[] { new SinDescuento() });
        var pedido = new PedidoBuilder(soloNormal)
            .ParaCliente(1, CategoriaCliente.Vip)
            .ConLinea(d.P01, 1)
            .ConEntrega(new FabricaEntregaEstandar())
            .EnFecha(Datos.Hoy)
            .Construir();

        pedido.AgregarLinea(d.P02, 1, soloNormal);

        Assert.Equal("NORMAL", pedido.NombreDescuento);
    }

    [Fact]
    public void R04_NoDebeCambiarseLaTarifaDeUnPedidoPagado()
    {
        var pedido = new Datos().Borrador();
        pedido.Validar(Datos.Hoy);
        pedido.Pagar(Datos.Hoy);

        Assert.Throws<DomainException>(() => pedido.CambiarEntrega(new FabricaEntregaExpres(), Datos.Calculadora()));
        Assert.Equal(5.00m, pedido.TarifaEnvio);
    }

    [Fact]
    public void R05_PrecioDeProductoDebeSerPositivo()
    {
        Assert.Throws<DomainException>(() => new ProductoSimple("P99", "Gratis", 0m, 1, 0.10m));
    }

    [Fact]
    public void R06_PedidoSinFormaDeEntregaNoDebeConstruirse()
    {
        var builder = Datos.Builder().ParaCliente(1, CategoriaCliente.Normal).ConLinea(new Datos().P01, 1);

        Assert.Throws<DomainException>(() => builder.Construir());
    }

    [Fact]
    public void R07_ReponerStockTambienDebeSubirLaVersion()
    {
        var p = new ProductoSimple("P01", "Mouse USB", 20.00m, 8, 0.20m);
        p.AgregarStock(2);

        Assert.Equal(1, p.Version);
    }

    [Fact]
    public async Task R08_KitLeidoDeLaBaseDebeCalcularSuPrecio()
    {
        using var lectura = _bd.NuevoContexto();
        var kit = await new KitRepository(lectura).ObtenerPorCodigoAsync("KIT_TELETRABAJO");

        Assert.Equal(60.00m, kit!.PrecioUnitario);
        Assert.Equal(new[] { "P01", "P02", "P03" }, kit.ObtenerComponentesFisicos().Select(c => c.Codigo));
    }

    [Fact]
    public async Task R09_UnitOfWorkNoDebeCerrarElContextoQueNoCreo()
    {
        await using var ctx = _bd.NuevoContexto();
        var uow = new UnitOfWork(ctx);
        await uow.GuardarCambiosAsync();

        Assert.False(typeof(IDisposable).IsAssignableFrom(typeof(UnitOfWork)));
        Assert.Equal(5, await ctx.Productos.CountAsync());
    }
}
