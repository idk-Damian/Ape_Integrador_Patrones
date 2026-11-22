using LogiMarket.Application;
using LogiMarket.Application.Comun;
using LogiMarket.Application.Pedidos;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Enums;
using LogiMarket.Infrastructure;
using LogiMarket.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Tests.Soporte;

public sealed class RelojControlado : IReloj
{
    public RelojControlado(DateTime inicio) => Ahora = inicio;
    public DateTime Ahora { get; private set; }
    public void Avanzar(TimeSpan tiempo) => Ahora = Ahora.Add(tiempo);
}

// composicion real (la misma DI de la aplicacion) sobre un archivo SQLite temporal y recien migrado
internal sealed class Entorno : IDisposable
{
    private readonly string _carpeta;

    public Entorno(Action<IServiceCollection>? ajustes = null)
    {
        _carpeta = Path.Combine(Path.GetTempPath(), "logimarket-pruebas", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_carpeta);
        RutaBase = Path.Combine(_carpeta, "prueba.db");

        var services = new ServiceCollection();
        services.AgregarInfraestructura($"Data Source={RutaBase};Pooling=False", _carpeta);
        services.AgregarAplicacion();
        services.AddSingleton<IReloj>(Reloj);
        ajustes?.Invoke(services);
        Proveedor = services.BuildServiceProvider();

        using var alcance = Proveedor.CreateScope();
        alcance.ServiceProvider.GetRequiredService<LogiMarketDbContext>().Database.Migrate();
        alcance.ServiceProvider.GetRequiredService<DatabaseSeeder>().SembrarAsync().GetAwaiter().GetResult();
    }

    public string RutaBase { get; }
    public string CarpetaSalidas => _carpeta;
    public ServiceProvider Proveedor { get; }
    public RelojControlado Reloj { get; } = new(new DateTime(2026, 11, 2, 10, 0, 0));
    public SesionUsuario Sesion => Proveedor.GetRequiredService<SesionUsuario>();
    public IControlSimulacion Control => Proveedor.GetRequiredService<IControlSimulacion>();
    public IBitacora Bitacora => Proveedor.GetRequiredService<IBitacora>();

    public void ComoRol(RolUsuario rol) => Sesion.Iniciar(rol.ToString().ToLowerInvariant(), rol);

    public async Task<T> EnAlcanceAsync<T>(Func<IServiceProvider, Task<T>> accion)
    {
        using var alcance = Proveedor.CreateScope();
        return await accion(alcance.ServiceProvider);
    }

    public Task<T> ConContextoAsync<T>(Func<LogiMarketDbContext, Task<T>> consulta)
        => EnAlcanceAsync(sp => consulta(sp.GetRequiredService<LogiMarketDbContext>()));

    public async Task<int> ClienteAsync(string nombre)
        => await ConContextoAsync(ctx => ctx.Clientes.Where(c => c.Nombre.StartsWith(nombre)).Select(c => c.Id).SingleAsync());

    // crea un borrador con la sesion de VENTAS y devuelve su id
    public async Task<int> BorradorAsync(string cliente, TipoEntrega entrega, params (string Codigo, int Cantidad)[] lineas)
    {
        var rolPrevio = Sesion.Rol;
        var nombrePrevio = Sesion.NombreUsuario;
        ComoRol(RolUsuario.Ventas);
        var clienteId = await ClienteAsync(cliente);
        var id = await EnAlcanceAsync(sp => sp.GetRequiredService<ServicioPedidos>()
            .CrearBorradorAsync(clienteId, lineas.Select(l => new LineaSolicitada(l.Codigo, l.Cantidad)).ToList(), entrega));
        if (rolPrevio is null) Sesion.Cerrar(); else Sesion.Iniciar(nombrePrevio, rolPrevio.Value);
        return id;
    }

    public Task<int> StockAsync(string sku)
        => ConContextoAsync(ctx => ctx.Productos.Where(p => p.Codigo == sku).Select(p => p.Stock).SingleAsync());

    public Task<int> PagosAsync() => ConContextoAsync(ctx => ctx.Pagos.CountAsync());

    public Task<int> EventosAsync(string tipo) => ConContextoAsync(ctx => ctx.EventosOutbox.CountAsync(e => e.TipoEvento == tipo));

    public Task<string> EstadoAsync(int pedidoId)
        => ConContextoAsync(ctx => ctx.Pedidos.Where(p => p.Id == pedidoId).Select(p => p.EstadoNombre).SingleAsync());

    public IReadOnlyList<string> MensajesPago()
        => Bitacora.Entradas(CategoriasBitacora.Pago).Select(e => e.Mensaje).ToList();

    public void Dispose()
    {
        Proveedor.Dispose();
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(_carpeta, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }
}
