using LogiMarket.Application.Excepciones;
using LogiMarket.Application.Interfaces;
using LogiMarket.Application.Seguridad;

namespace LogiMarket.Application.Comprobantes;

// arma los datos y combina el tipo de comprobante con el medio elegido (2 x 2 sin clases de combinacion)
public sealed class ServicioComprobantes
{
    private readonly IPedidoRepository _pedidos;
    private readonly IClienteRepository _clientes;
    private readonly IPagoRepository _pagos;
    private readonly IDevolucionRepository _devoluciones;
    private readonly IReadOnlyDictionary<string, ISalidaDocumento> _salidas;
    private readonly IContextoUsuario _usuario;

    public ServicioComprobantes(IPedidoRepository pedidos, IClienteRepository clientes, IPagoRepository pagos,
        IDevolucionRepository devoluciones, IEnumerable<ISalidaDocumento> salidas, IContextoUsuario usuario)
    {
        _pedidos = pedidos;
        _clientes = clientes;
        _pagos = pagos;
        _devoluciones = devoluciones;
        _salidas = salidas.ToDictionary(s => s.Medio);
        _usuario = usuario;
    }

    public IReadOnlyCollection<string> Medios => _salidas.Keys.ToList();

    public async Task<ResultadoSalida> EmitirVentaAsync(int pedidoId, string medio)
    {
        PoliticaPermisos.Exigir(_usuario, Operacion.ConsultarInformacion);
        var pedido = await _pedidos.ObtenerPorIdAsync(pedidoId) ?? throw new RecursoNoEncontradoException($"No existe el pedido #{pedidoId}.");
        var pago = await _pagos.ObtenerPorPedidoAsync(pedidoId) ?? throw new RecursoNoEncontradoException("El pedido aún no fue pagado.");
        var cliente = await _clientes.ObtenerPorIdAsync(pedido.ClienteId);

        var datos = new DatosVenta(pedido.Id, cliente?.Nombre ?? "-", cliente?.Email ?? "-", pago.Fecha,
            pedido.Lineas.Select(l => new LineaComprobante(l.ElementoCodigo, l.ElementoNombre, l.Cantidad, l.PrecioUnitario, l.Subtotal)).ToList(),
            pedido.Subtotal, pedido.NombreDescuento, pedido.Descuento, pedido.TipoEntrega.ToString().ToUpperInvariant(),
            pedido.TarifaEnvio, pedido.Total, pago.ReferenciaCobro);

        return new ComprobanteVenta(Salida(medio), datos).Emitir();
    }

    public async Task<ResultadoSalida> EmitirDevolucionAsync(int pedidoId, string medio)
    {
        PoliticaPermisos.Exigir(_usuario, Operacion.ConsultarInformacion);
        var devolucion = await _devoluciones.ObtenerPorPedidoAsync(pedidoId)
            ?? throw new RecursoNoEncontradoException($"El pedido #{pedidoId} no tiene devolución.");
        var pedido = await _pedidos.ObtenerPorIdAsync(pedidoId) ?? throw new RecursoNoEncontradoException($"No existe el pedido #{pedidoId}.");
        var cliente = await _clientes.ObtenerPorIdAsync(pedido.ClienteId);

        var datos = new DatosDevolucion(pedido.Id, cliente?.Nombre ?? "-", devolucion.Fecha, devolucion.Motivo.ToString().ToUpperInvariant(),
            devolucion.DiasDesdeEntrega, pedido.Total, pedido.TarifaEnvio, devolucion.MontoReembolso, devolucion.ReferenciaReembolso);

        return new ComprobanteDevolucion(Salida(medio), datos).Emitir();
    }

    private ISalidaDocumento Salida(string medio)
        => _salidas.TryGetValue(medio, out var salida) ? salida : throw new RecursoNoEncontradoException($"Medio de salida desconocido: {medio}.");
}
