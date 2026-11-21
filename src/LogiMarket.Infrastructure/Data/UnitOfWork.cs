using LogiMarket.Application.Interfaces;
using LogiMarket.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace LogiMarket.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly LogiMarketDbContext _context;
    private IDbContextTransaction? _transaccion;

    public IProductoRepository Productos { get; }
    public IClienteRepository Clientes { get; }
    public IPedidoRepository Pedidos { get; }
    public IUsuarioRepository Usuarios { get; }
    public IKitRepository Kits { get; }
    public IPagoRepository Pagos { get; }
    public IDespachoRepository Despachos { get; }
    public IDevolucionRepository Devoluciones { get; }
    public IMovimientoInventarioRepository MovimientosInventario { get; }
    public IEventoOutboxRepository EventosOutbox { get; }
    public IAuditoriaRepository Auditorias { get; }

    public UnitOfWork(LogiMarketDbContext context)
    {
        _context = context;
        Productos = new ProductoRepository(context);
        Clientes = new ClienteRepository(context);
        Pedidos = new PedidoRepository(context);
        Usuarios = new UsuarioRepository(context);
        Kits = new KitRepository(context);
        Pagos = new PagoRepository(context);
        Despachos = new DespachoRepository(context);
        Devoluciones = new DevolucionRepository(context);
        MovimientosInventario = new MovimientoInventarioRepository(context);
        EventosOutbox = new EventoOutboxRepository(context);
        Auditorias = new AuditoriaRepository(context);
    }

    public UnitOfWork(
        LogiMarketDbContext context,
        IProductoRepository productos,
        IClienteRepository clientes,
        IPedidoRepository pedidos,
        IUsuarioRepository usuarios,
        IKitRepository kits,
        IPagoRepository pagos,
        IDespachoRepository despachos,
        IDevolucionRepository devoluciones,
        IMovimientoInventarioRepository movimientosInventario,
        IEventoOutboxRepository eventosOutbox,
        IAuditoriaRepository auditorias)
    {
        _context = context;
        Productos = productos;
        Clientes = clientes;
        Pedidos = pedidos;
        Usuarios = usuarios;
        Kits = kits;
        Pagos = pagos;
        Despachos = despachos;
        Devoluciones = devoluciones;
        MovimientosInventario = movimientosInventario;
        EventosOutbox = eventosOutbox;
        Auditorias = auditorias;
    }

    public async Task<int> GuardarCambiosAsync()
        => await _context.SaveChangesAsync();

    public async Task IniciarTransaccionAsync()
    {
        _transaccion = await _context.Database.BeginTransactionAsync();
    }

    public async Task ConfirmarTransaccionAsync()
    {
        if (_transaccion is not null)
        {
            await _transaccion.CommitAsync();
            await _transaccion.DisposeAsync();
            _transaccion = null;
        }
    }

    public async Task RevertirTransaccionAsync()
    {
        if (_transaccion is not null)
        {
            await _transaccion.RollbackAsync();
            await _transaccion.DisposeAsync();
            _transaccion = null;
        }
    }

    public void Dispose()
    {
        _transaccion?.Dispose();
        _context.Dispose();
    }
}
