using LogiMarket.Application.Interfaces;
using LogiMarket.Infrastructure.Data;
using LogiMarket.Infrastructure.Repositories;
using LogiMarket.Infrastructure.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Infrastructure;

// registro de adaptadores; lo usa el composition root de WinForms y las pruebas de integracion
public static class DependencyInjection
{
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection services, string cadenaConexion)
    {
        services.AddDbContext<LogiMarketDbContext>(o => o.UseSqlite(cadenaConexion));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IKitRepository, KitRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<IPagoRepository, PagoRepository>();
        services.AddScoped<IMovimientoInventarioRepository, MovimientoInventarioRepository>();
        services.AddScoped<IDespachoRepository, DespachoRepository>();
        services.AddScoped<IDevolucionRepository, DevolucionRepository>();
        services.AddScoped<IEventoOutboxRepository, EventoOutboxRepository>();
        services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
