namespace LogiMarket.Application.Interfaces;

/// <summary>
/// Unit of Work respaldado por transacción real de DbContext.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IProductoRepository Productos { get; }
    IClienteRepository Clientes { get; }
    IPedidoRepository Pedidos { get; }
    IUsuarioRepository Usuarios { get; }
    IKitRepository Kits { get; }
    IPagoRepository Pagos { get; }
    IDespachoRepository Despachos { get; }
    IDevolucionRepository Devoluciones { get; }
    IMovimientoInventarioRepository MovimientosInventario { get; }
    IEventoOutboxRepository EventosOutbox { get; }
    IAuditoriaRepository Auditorias { get; }

    Task<int> GuardarCambiosAsync();
    Task IniciarTransaccionAsync();
    Task ConfirmarTransaccionAsync();
    Task RevertirTransaccionAsync();
}
