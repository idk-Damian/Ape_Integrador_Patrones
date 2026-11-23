using LogiMarket.Application.Excepciones;
using LogiMarket.Domain.Entregas;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Pagos;
using LogiMarket.Domain.Pedidos.Descuentos;
using LogiMarket.Infrastructure.Data;
using LogiMarket.Infrastructure.Repositories;
using LogiMarket.Tests.Soporte;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Tests.Persistencia;

public class PersistenciaTests : IDisposable
{
    private readonly BaseDePruebas _bd = new();

    public void Dispose() => _bd.Dispose();

    [Fact]
    public async Task P01_MigracionesYSemillaSinDuplicados()
    {
        using (var ctx = _bd.NuevoContexto())
            await new DatabaseSeeder(ctx, _bd.Hasher, _bd.Reloj).SembrarAsync();

        using var lectura = _bd.NuevoContexto();
        Assert.Empty(await lectura.Database.GetPendingMigrationsAsync());
        Assert.Equal(5, await lectura.Productos.CountAsync());
        Assert.Equal(2, await lectura.Kits.CountAsync());
        Assert.Equal(4, await lectura.Usuarios.CountAsync());
        Assert.Equal(3, await lectura.Clientes.CountAsync());
        Assert.Equal(5, await lectura.MovimientosInventario.CountAsync());

        var stock = await lectura.Productos.OrderBy(p => p.Codigo).Select(p => p.Stock).ToListAsync();
        Assert.Equal(new[] { 8, 5, 20, 4, 30 }, stock);

        var admin = await lectura.Usuarios.SingleAsync(u => u.NombreUsuario == "admin");
        Assert.True(_bd.Hasher.Verificar("Admin123!", admin.PasswordHash, admin.Salt));
        Assert.False(_bd.Hasher.Verificar("admin123!", admin.PasswordHash, admin.Salt));
    }

    [Fact]
    public async Task P10_PedidoSeGuardaYSeRecuperaConLineasEHistorial()
    {
        int pedidoId;
        using (var ctx = _bd.NuevoContexto())
        {
            var kit = await new KitRepository(ctx).ObtenerPorCodigoAsync("KIT_TELETRABAJO");
            var ana = await ctx.Clientes.SingleAsync(c => c.Email == "ana@demo.local");
            var pedido = Datos.Builder().ParaCliente(ana).ConLinea(kit!, 1).ConEntrega(new FabricaEntregaEstandar()).Construir();
            pedido.Validar(Datos.Hoy);

            await new PedidoRepository(ctx).AgregarAsync(pedido);
            await new UnitOfWork(ctx).GuardarCambiosAsync();
            pedidoId = pedido.Id;
        }

        using var lectura = _bd.NuevoContexto();
        var guardado = await new PedidoRepository(lectura).ObtenerPorIdAsync(pedidoId);

        Assert.NotNull(guardado);
        Assert.Equal("VALIDADO", guardado!.EstadoNombre);
        Assert.Equal(59.00m, guardado.Total);
        Assert.Equal(TipoEntrega.Estandar, guardado.TipoEntrega);
        Assert.Equal(new[] { "P01", "P02", "P03" }, guardado.Lineas[0].Componentes.Select(c => c.ProductoCodigo));
        Assert.Equal(2, guardado.CambiosEstado.Count);
        var requerimiento = guardado.RequerimientoPorSku();
        Assert.Equal(3, requerimiento.Count);
        Assert.All(new[] { "P01", "P02", "P03" }, sku => Assert.Equal(1, requerimiento[sku]));
    }

    [Fact]
    public async Task P10_CambioDePrecioPosteriorNoAlteraLaInstantanea()
    {
        int pedidoId;
        using (var ctx = _bd.NuevoContexto())
        {
            var p04 = await new ProductoRepository(ctx).ObtenerPorCodigoAsync("P04");
            var luis = await ctx.Clientes.SingleAsync(c => c.Email == "luis@demo.local");
            var pedido = Datos.Builder().ParaCliente(luis).ConLinea(p04!, 2).ConEntrega(new FabricaEntregaExpres()).Construir();
            await ctx.Pedidos.AddAsync(pedido);
            await ctx.SaveChangesAsync();
            pedidoId = pedido.Id;

            p04!.ActualizarPrecio(99.00m);
            await ctx.SaveChangesAsync();
        }

        using var lectura = _bd.NuevoContexto();
        var guardado = await new PedidoRepository(lectura).ObtenerPorIdAsync(pedidoId);
        Assert.Equal(40.00m, guardado!.Lineas[0].PrecioUnitario);
        Assert.Equal(92.00m, guardado.Total);
    }

    [Fact]
    public async Task P36_ConcurrenciaOptimistaEvitaSobreventa()
    {
        using var ctxA = _bd.NuevoContexto();
        using var ctxB = _bd.NuevoContexto();
        var a = await new ProductoRepository(ctxA).ObtenerPorCodigoAsync("P04");
        var b = await new ProductoRepository(ctxB).ObtenerPorCodigoAsync("P04");

        a!.DescontarStock(3);
        await new UnitOfWork(ctxA).GuardarCambiosAsync();

        b!.DescontarStock(3);
        await Assert.ThrowsAsync<ConflictoConcurrenciaException>(() => new UnitOfWork(ctxB).GuardarCambiosAsync());

        using var lectura = _bd.NuevoContexto();
        Assert.Equal(1, (await lectura.Productos.SingleAsync(p => p.Codigo == "P04")).Stock);
    }

    [Fact]
    public async Task TransaccionRevertidaNoDejaCambios()
    {
        using (var ctx = _bd.NuevoContexto())
        {
            var uow = new UnitOfWork(ctx);
            await using var tx = await uow.IniciarTransaccionAsync();
            var p01 = await new ProductoRepository(ctx).ObtenerPorCodigoAsync("P01");
            p01!.DescontarStock(8);
            await uow.GuardarCambiosAsync();
            await tx.RevertirAsync();
        }

        using var lectura = _bd.NuevoContexto();
        Assert.Equal(8, (await lectura.Productos.SingleAsync(p => p.Codigo == "P01")).Stock);
    }

    [Fact]
    public async Task P07_ClaveDeIdempotenciaRepetidaSeRechazaPorUnicidad()
    {
        using var ctx = _bd.NuevoContexto();
        var luis = await ctx.Clientes.SingleAsync(c => c.Email == "luis@demo.local");
        var p05 = await ctx.Productos.SingleAsync(p => p.Codigo == "P05");
        var uno = Datos.Builder().ParaCliente(luis).ConLinea(p05, 1).ConEntrega(new FabricaEntregaEstandar()).Construir();
        var dos = Datos.Builder().ParaCliente(luis).ConLinea(p05, 1).ConEntrega(new FabricaEntregaEstandar()).Construir();
        await ctx.Pedidos.AddRangeAsync(uno, dos);
        await ctx.SaveChangesAsync();

        var uow = new UnitOfWork(ctx);
        await ctx.Pagos.AddAsync(new Pago(uno.Id, 10m, "LEGADO", "REF-1", "CLAVE-1", DateTime.UtcNow));
        await uow.GuardarCambiosAsync();

        await ctx.Pagos.AddAsync(new Pago(dos.Id, 10m, "LEGADO", "REF-2", "CLAVE-1", DateTime.UtcNow));
        await Assert.ThrowsAsync<ConflictoDuplicadoException>(() => uow.GuardarCambiosAsync());
    }
}
