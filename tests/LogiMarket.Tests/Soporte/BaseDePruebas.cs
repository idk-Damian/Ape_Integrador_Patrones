using LogiMarket.Infrastructure.Data;
using LogiMarket.Infrastructure.Seguridad;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Tests.Soporte;

// SQLite en memoria con las migraciones reales aplicadas; cada prueba arranca de cero
internal sealed class BaseDePruebas : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<LogiMarketDbContext> _opciones;

    public BaseDePruebas(bool sembrar = true)
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();
        _opciones = new DbContextOptionsBuilder<LogiMarketDbContext>().UseSqlite(_conexion).Options;

        using var ctx = NuevoContexto();
        ctx.Database.Migrate();
        if (sembrar) new DatabaseSeeder(ctx, Hasher, Reloj).SembrarAsync().GetAwaiter().GetResult();
    }

    public Pbkdf2PasswordHasher Hasher { get; } = new();

    public RelojControlado Reloj { get; } = new(new DateTime(2026, 11, 2, 10, 0, 0));

    public LogiMarketDbContext NuevoContexto() => new(_opciones);

    public void Dispose() => _conexion.Dispose();
}
