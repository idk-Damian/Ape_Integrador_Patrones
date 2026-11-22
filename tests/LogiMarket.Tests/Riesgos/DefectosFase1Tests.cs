using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.Pedidos;
using LogiMarket.Domain.Pedidos.Descuentos;
using LogiMarket.Infrastructure.Data;
using LogiMarket.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Tests.Riesgos;

// pruebas que dejan a la vista los defectos encontrados al revisar la fase 1
// se escriben antes de corregir para tener el "antes" de cada refactorizacion
public class DefectosFase1Tests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<LogiMarketDbContext> _opciones;

    public DefectosFase1Tests()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();
        _opciones = new DbContextOptionsBuilder<LogiMarketDbContext>().UseSqlite(_conexion).Options;
        using var ctx = new LogiMarketDbContext(_opciones);
        ctx.Database.EnsureCreated();
    }

    public void Dispose() => _conexion.Dispose();

    private static (ProductoSimple p01, ProductoSimple p02, ProductoSimple p03, KitCompuesto oficina) Catalogo()
    {
        var p01 = new ProductoSimple("P01", "Mouse USB", 20.00m, 8, 0.20m);
        var p02 = new ProductoSimple("P02", "Teclado", 30.00m, 5, 0.70m);
        var p03 = new ProductoSimple("P03", "Memoria USB", 10.00m, 20, 0.10m);
        var oficina = new KitCompuesto("KIT_OFICINA", "Kit Oficina");
        oficina.AgregarComponente(p01, 1);
        oficina.AgregarComponente(p02, 1);
        return (p01, p02, p03, oficina);
    }

    [Fact]
    public void R01_Iterador_NoDebeReportarCicloSiUnKitApareceEnDosRamas()
    {
        var (_, _, p03, oficina) = Catalogo();
        var rama = new KitCompuesto("KIT_RAMA", "Rama");
        rama.AgregarComponente(oficina, 1);
        rama.AgregarComponente(p03, 1);
        var raiz = new KitCompuesto("KIT_RAIZ", "Raiz");
        raiz.AgregarComponente(oficina, 1);
        raiz.AgregarComponente(rama, 1);

        var porComposite = raiz.ObtenerComponentesFisicos().Sum(c => c.Cantidad);
        var porIterador = new IteradorComponentesFisicos(raiz).ToList().Sum(c => c.Cantidad);

        Assert.Equal(porComposite, porIterador);
    }

    [Fact]
    public void R03_EstrategiaElegidaNoDebePerderseAlEditarElBorrador()
    {
        var (p01, p02, _, _) = Catalogo();
        var pedido = new PedidoBuilder()
            .ConCliente(1, CategoriaCliente.Vip)
            .AgregarProducto(p01, 1)
            .ConEstrategiaDescuento(new SinDescuento())
            .Construir();

        pedido.AgregarLinea(new LineaPedido(p02, 1));

        Assert.Equal("NORMAL", pedido.NombreDescuento);
    }

    [Fact]
    public void R04_NoDebeCambiarseLaTarifaDeUnPedidoPagado()
    {
        var (p01, _, _, _) = Catalogo();
        var pedido = new PedidoBuilder().ConCliente(1, CategoriaCliente.Normal).AgregarProducto(p01, 1).Construir();
        pedido.Validar();
        pedido.Pagar();

        Assert.Throws<DomainException>(() => pedido.EstablecerTarifaEnvio(50m));
    }

    [Fact]
    public void R05_PrecioDeProductoDebeSerPositivo()
    {
        Assert.Throws<DomainException>(() => new ProductoSimple("P99", "Gratis", 0m, 1, 0.10m));
    }

    [Fact]
    public void R06_PedidoSinFormaDeEntregaNoDebeConstruirse()
    {
        var (p01, _, _, _) = Catalogo();
        var builder = new PedidoBuilder().ConCliente(1, CategoriaCliente.Normal).AgregarProducto(p01, 1);

        Assert.Throws<DomainException>(() => builder.Construir());
    }

    [Fact]
    public void R07_ReponerStockTambienDebeSubirLaVersion()
    {
        var p = new ProductoSimple("P01", "Mouse USB", 20.00m, 8, 0.20m);
        p.AgregarStock(2);

        Assert.Equal(1u, p.Version);
    }

    [Fact]
    public async Task R08_KitLeidoDeLaBaseDebeCalcularSuPrecio()
    {
        using (var ctx = new LogiMarketDbContext(_opciones))
            await DatabaseSeeder.SeedAsync(ctx);

        using var lectura = new LogiMarketDbContext(_opciones);
        var kit = await new KitRepository(lectura).ObtenerPorCodigoAsync("KIT_TELETRABAJO");

        Assert.Equal(60.00m, kit!.PrecioUnitario);
    }

    [Fact]
    public async Task R09_UnitOfWorkNoDebeCerrarElContextoQueNoCreo()
    {
        var ctx = new LogiMarketDbContext(_opciones);
        var uow = new UnitOfWork(ctx);
        uow.Dispose();

        var cuenta = await ctx.Productos.CountAsync();
        Assert.True(cuenta >= 0);
        await ctx.DisposeAsync();
    }
}
