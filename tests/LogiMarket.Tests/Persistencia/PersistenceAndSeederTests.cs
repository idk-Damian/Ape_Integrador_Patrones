using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Common;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Usuarios;
using LogiMarket.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LogiMarket.Tests.Persistencia;

public class PersistenceAndSeederTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<LogiMarketDbContext> _options;

    public PersistenceAndSeederTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<LogiMarketDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new LogiMarketDbContext(_options);
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    [Fact]
    public async Task DatabaseSeeder_EsIdempotente_NoDuplicaRegistros()
    {
        using (var context = new LogiMarketDbContext(_options))
        {
            await DatabaseSeeder.SeedAsync(context);
        }

        // Ejecutar de nuevo para comprobar idempotencia
        using (var context = new LogiMarketDbContext(_options))
        {
            await DatabaseSeeder.SeedAsync(context);
        }

        using (var context = new LogiMarketDbContext(_options))
        {
            var productos = await context.Productos.ToListAsync();
            var kits = await context.Kits.ToListAsync();
            var usuarios = await context.Usuarios.ToListAsync();
            var clientes = await context.Clientes.ToListAsync();

            Assert.Equal(5, productos.Count);
            Assert.Equal(2, kits.Count);
            Assert.Equal(4, usuarios.Count);
            Assert.Equal(3, clientes.Count);

            // Verificar suma exacta de 1.00 kg para P01 + P02 + P03
            var p01 = productos.First(p => p.Codigo == "P01");
            var p02 = productos.First(p => p.Codigo == "P02");
            var p03 = productos.First(p => p.Codigo == "P03");
            Assert.Equal(1.00m, p01.PesoKg + p02.PesoKg + p03.PesoKg);

            // Verificar contraseñas hasheadas (no texto plano)
            var admin = usuarios.First(u => u.NombreUsuario == "admin");
            Assert.True(admin.ValidarPassword("Admin123!"));
            Assert.False(admin.ValidarPassword("PasswordIncorrecto"));
        }
    }

    [Fact]
    public async Task ConcurrenciaOptimista_DetectaSobreventa()
    {
        using (var context1 = new LogiMarketDbContext(_options))
        {
            var prod = new ProductoSimple("TEST01", "Test Concurrencia", 10.00m, 5, 0.10m);
            await context1.Productos.AddAsync(prod);
            await context1.SaveChangesAsync();
        }

        // Simular dos transacciones concurrentes
        using var contextA = new LogiMarketDbContext(_options);
        using var contextB = new LogiMarketDbContext(_options);

        var prodA = await contextA.Productos.FirstAsync(p => p.Codigo == "TEST01");
        var prodB = await contextB.Productos.FirstAsync(p => p.Codigo == "TEST01");

        // Usuario A descuenta stock
        prodA.DescontarStock(3);
        await contextA.SaveChangesAsync();

        // Usuario B intenta descontar stock sobre versión desactualizada
        prodB.DescontarStock(3);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
        {
            await contextB.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task UnitOfWork_TransaccionReal_CommitYRollback()
    {
        using (var context = new LogiMarketDbContext(_options))
        {
            using var uow = new UnitOfWork(context);

            await uow.IniciarTransaccionAsync();
            await uow.Productos.AgregarAsync(new ProductoSimple("TX01", "Prod TX", 10.00m, 5, 0.10m));
            await uow.GuardarCambiosAsync();
            await uow.RevertirTransaccionAsync();
        }

        using (var context = new LogiMarketDbContext(_options))
        {
            var existe = await context.Productos.AnyAsync(p => p.Codigo == "TX01");
            Assert.False(existe);
        }
    }
}
