using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Clientes;
using LogiMarket.Domain.Despachos;
using LogiMarket.Domain.Devoluciones;
using LogiMarket.Domain.Inventario;
using LogiMarket.Domain.Outbox;
using LogiMarket.Domain.Pagos;
using LogiMarket.Domain.Pedidos;
using LogiMarket.Domain.Pedidos.Historial;
using LogiMarket.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Data;

public class LogiMarketDbContext : DbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ProductoSimple> Productos => Set<ProductoSimple>();
    public DbSet<KitCompuesto> Kits => Set<KitCompuesto>();
    public DbSet<ComponenteKit> KitComponentes => Set<ComponenteKit>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<LineaPedido> PedidoDetalles => Set<LineaPedido>();
    public DbSet<LineaComponenteSnapshot> PedidoDetalleComponentes => Set<LineaComponenteSnapshot>();
    public DbSet<CambioEstadoPedido> PedidoHistorialEstados => Set<CambioEstadoPedido>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<Despacho> Despachos => Set<Despacho>();
    public DbSet<Devolucion> Devoluciones => Set<Devolucion>();
    public DbSet<EventoOutbox> EventosOutbox => Set<EventoOutbox>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    public LogiMarketDbContext(DbContextOptions<LogiMarketDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(LogiMarketDbContext).Assembly);
}
