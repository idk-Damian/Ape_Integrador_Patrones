using LogiMarket.Application.Mantenimiento;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Infrastructure.Data;

// crea la base desde cero con las migraciones y la semilla; restablecer la borra y repite el proceso
public sealed class MantenimientoBaseDatos : IMantenimientoBaseDatos
{
    private readonly IServiceScopeFactory _alcances;

    public MantenimientoBaseDatos(IServiceScopeFactory alcances, string ubicacion)
    {
        _alcances = alcances;
        Ubicacion = ubicacion;
    }

    public string Ubicacion { get; }

    public async Task InicializarAsync()
    {
        var carpeta = Path.GetDirectoryName(Path.GetFullPath(Ubicacion));
        if (!string.IsNullOrEmpty(carpeta)) Directory.CreateDirectory(carpeta);

        using var alcance = _alcances.CreateScope();
        await alcance.ServiceProvider.GetRequiredService<LogiMarketDbContext>().Database.MigrateAsync();
        await alcance.ServiceProvider.GetRequiredService<DatabaseSeeder>().SembrarAsync();
    }

    public async Task RestablecerAsync()
    {
        SqliteConnection.ClearAllPools();
        using (var alcance = _alcances.CreateScope())
            await alcance.ServiceProvider.GetRequiredService<LogiMarketDbContext>().Database.EnsureDeletedAsync();
        await InicializarAsync();
    }
}
