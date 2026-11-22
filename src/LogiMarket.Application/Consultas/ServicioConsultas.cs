using LogiMarket.Application.Comun;
using LogiMarket.Application.Excepciones;
using LogiMarket.Application.Interfaces;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Enums;

namespace LogiMarket.Application.Consultas;

public sealed record ConteoEstado(string Estado, int Pedidos, decimal Total);
public sealed record AlertaStock(string Sku, string Nombre, int Stock);
public sealed record PanelResumen(IReadOnlyList<ConteoEstado> PorEstado, IReadOnlyList<AlertaStock> StockBajo, decimal VentasCobradas,
    int EventosPendientes);

public sealed record LineaDetalle(string Codigo, string Descripcion, int Cantidad, decimal PrecioUnitario, decimal Subtotal, string Componentes);
public sealed record CambioEstadoDto(int Secuencia, string? Desde, string Hacia, DateTime Fecha);
public sealed record DetallePedido(PedidoResumen Resumen, decimal Subtotal, string Politica, decimal Descuento, decimal Tarifa,
    IReadOnlyList<LineaDetalle> Lineas, IReadOnlyList<CambioEstadoDto> Historial, string? ReferenciaCobro, string? Guia,
    DateTime? FechaEstimada, DateTime? FechaEntrega, decimal? Reembolso, decimal ImporteReembolsable);

public sealed record MovimientoDto(int Id, DateTime Fecha, string Sku, string Tipo, int Cantidad, int StockResultante, string Motivo, string Referencia);
public sealed record AuditoriaDto(DateTime Fecha, string Categoria, string Accion, string Referencia, string Usuario, string Detalle);
public sealed record EventoDto(int Id, string Tipo, DateTime Creado, string Estado, int Intentos, string Error);

// consultas de solo lectura para las pantallas; ninguna modifica datos
public sealed class ServicioConsultas
{
    public const int UmbralStockBajo = 3;

    private readonly IConsultasPedidos _resumenes;
    private readonly IPedidoRepository _pedidos;
    private readonly IProductoRepository _productos;
    private readonly IPagoRepository _pagos;
    private readonly IDespachoRepository _despachos;
    private readonly IDevolucionRepository _devoluciones;
    private readonly IMovimientoInventarioRepository _movimientos;
    private readonly IAuditoriaRepository _auditoria;
    private readonly IEventoOutboxRepository _outbox;
    private readonly IContextoUsuario _usuario;

    public ServicioConsultas(IConsultasPedidos resumenes, IPedidoRepository pedidos, IProductoRepository productos, IPagoRepository pagos,
        IDespachoRepository despachos, IDevolucionRepository devoluciones, IMovimientoInventarioRepository movimientos,
        IAuditoriaRepository auditoria, IEventoOutboxRepository outbox, IContextoUsuario usuario)
    {
        _resumenes = resumenes;
        _pedidos = pedidos;
        _productos = productos;
        _pagos = pagos;
        _despachos = despachos;
        _devoluciones = devoluciones;
        _movimientos = movimientos;
        _auditoria = auditoria;
        _outbox = outbox;
        _usuario = usuario;
    }

    public async Task<PanelResumen> PanelAsync()
    {
        Exigir();
        var pedidos = await _resumenes.ListarResumenAsync();
        var estados = new[] { "BORRADOR", "VALIDADO", "PAGADO", "DESPACHADO", "ENTREGADO", "CANCELADO" };
        var conteo = estados.Select(e => new ConteoEstado(e, pedidos.Count(p => p.Estado == e),
            pedidos.Where(p => p.Estado == e).Sum(p => p.Total))).ToList();
        var bajos = (await _productos.ListarAsync()).Where(p => p.Stock <= UmbralStockBajo)
            .Select(p => new AlertaStock(p.Codigo, p.Nombre, p.Stock)).ToList();
        var cobradas = pedidos.Where(p => p.Estado is "PAGADO" or "DESPACHADO" or "ENTREGADO").Sum(p => p.Total);
        var pendientes = (await _outbox.ObtenerPendientesAsync(500)).Count;
        return new PanelResumen(conteo, bajos, cobradas, pendientes);
    }

    public async Task<IReadOnlyList<PedidoResumen>> PedidosEnEstadoAsync(params string[] estados)
    {
        Exigir();
        return (await _resumenes.ListarResumenAsync()).Where(p => estados.Contains(p.Estado)).ToList();
    }

    public async Task<DetallePedido> DetalleAsync(int pedidoId)
    {
        Exigir();
        var pedido = await _pedidos.ObtenerPorIdAsync(pedidoId) ?? throw new RecursoNoEncontradoException($"No existe el pedido #{pedidoId}.");
        var resumen = (await _resumenes.ListarResumenAsync()).First(r => r.Id == pedidoId);
        var pago = await _pagos.ObtenerPorPedidoAsync(pedidoId);
        var despacho = await _despachos.ObtenerPorPedidoAsync(pedidoId);
        var devolucion = await _devoluciones.ObtenerPorPedidoAsync(pedidoId);

        var lineas = pedido.Lineas.Select(l => new LineaDetalle(l.ElementoCodigo, l.ElementoNombre, l.Cantidad, l.PrecioUnitario,
            l.Subtotal, string.Join(", ", l.Componentes.Select(c => $"{c.ProductoCodigo} x{c.Cantidad * l.Cantidad}")))).ToList();
        var historial = pedido.CambiosEstado.Select(c => new CambioEstadoDto(c.Secuencia, c.EstadoAnterior, c.EstadoNuevo, c.Fecha)).ToList();

        return new DetallePedido(resumen, pedido.Subtotal, pedido.NombreDescuento, pedido.Descuento, pedido.TarifaEnvio, lineas, historial,
            pago?.ReferenciaCobro, despacho?.NumeroGuia, despacho?.FechaEstimada, pedido.FechaEntrega, devolucion?.MontoReembolso,
            pedido.ImporteProductos);
    }

    public async Task<IReadOnlyList<MovimientoDto>> MovimientosAsync()
    {
        Exigir();
        var skus = (await _productos.ListarAsync()).ToDictionary(p => p.Id, p => p.Codigo);
        return (await _movimientos.ListarAsync()).Select(m => new MovimientoDto(m.Id, m.Fecha, skus.GetValueOrDefault(m.ProductoId, "?"),
            m.Tipo.ToString(), m.Cantidad, m.StockResultante, m.Motivo, m.Referencia)).ToList();
    }

    public async Task<IReadOnlyList<AuditoriaDto>> AuditoriaAsync()
    {
        Exigir();
        return (await _auditoria.ListarAsync()).Select(a => new AuditoriaDto(a.Fecha, a.Categoria, a.Accion, a.Referencia, a.Usuario,
            a.Detalle ?? "")).ToList();
    }

    public async Task<IReadOnlyList<EventoDto>> EventosAsync()
    {
        Exigir();
        return (await _outbox.ListarAsync()).Select(e => new EventoDto(e.Id, e.TipoEvento, e.FechaCreacion,
            e.Procesado ? "PROCESADO" : "PENDIENTE", e.Intentos, e.UltimoError ?? "")).ToList();
    }

    private void Exigir() => PoliticaPermisos.Exigir(_usuario, Operacion.ConsultarInformacion);
}
