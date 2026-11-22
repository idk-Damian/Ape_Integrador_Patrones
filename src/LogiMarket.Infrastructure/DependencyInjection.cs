using LogiMarket.Application.Comun;
using LogiMarket.Application.Interfaces;
using LogiMarket.Application.Pagos;
using LogiMarket.Application.Seguridad;
using LogiMarket.Infrastructure.Comun;
using LogiMarket.Infrastructure.Data;
using LogiMarket.Infrastructure.Pagos;
using LogiMarket.Infrastructure.Pagos.Legado;
using LogiMarket.Infrastructure.Repositories;
using LogiMarket.Infrastructure.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Infrastructure;

// registro de adaptadores; lo usa el composition root de WinForms y las pruebas de integracion
public static class DependencyInjection
{
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection services, string cadenaConexion,
        string carpetaSalidas)
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

        services.AddSingleton<IReloj, RelojSistema>();
        services.AddSingleton<IControlSimulacion, ControlSimulacion>();
        services.AddSingleton<IBitacora>(sp => new BitacoraTecnica(sp.GetRequiredService<IReloj>(),
            Path.Combine(carpetaSalidas, "logs")));

        // proveedores de cobro simulados
        services.AddSingleton<PasarelaLegadaSimulada>();
        services.AddSingleton<PasarelaLegadaAdapter>();
        services.AddSingleton<PasarelaAlternativaSimulada>();

        // orden fijo: Proxy -> Decorator -> proveedor elegido en el panel de demostracion
        services.AddScoped<IPasarelaPago>(sp =>
        {
            var control = sp.GetRequiredService<IControlSimulacion>();
            var bitacora = sp.GetRequiredService<IBitacora>();
            IPasarelaPago proveedor = control.ProveedorPago == ProveedoresPago.Alternativa
                ? sp.GetRequiredService<PasarelaAlternativaSimulada>()
                : sp.GetRequiredService<PasarelaLegadaAdapter>();

            return new PagoAutorizadoProxy(
                new AuditoriaPagoDecorator(proveedor, bitacora),
                sp.GetRequiredService<IContextoUsuario>(),
                bitacora);
        });

        return services;
    }
}
