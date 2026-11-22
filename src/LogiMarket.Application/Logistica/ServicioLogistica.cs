using System.Text.Json;
using LogiMarket.Application.Comun;
using LogiMarket.Application.Entregas;
using LogiMarket.Application.Eventos;
using LogiMarket.Application.Excepciones;
using LogiMarket.Application.Interfaces;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Despachos;
using LogiMarket.Domain.Outbox;

namespace LogiMarket.Application.Logistica;

public sealed class ServicioLogistica
{
    public const string EventoDespachoRegistrado = TiposEvento.DespachoRegistrado;

    private readonly IPedidoRepository _pedidos;
    private readonly IDespachoRepository _despachos;
    private readonly IClienteRepository _clientes;
    private readonly IEventoOutboxRepository _outbox;
    private readonly IUnitOfWork _uow;
    private readonly CatalogoFabricasEntrega _entregas;
    private readonly IContextoUsuario _usuario;
    private readonly IReloj _reloj;
    private readonly EjecutorOperaciones _ejecutor;

    public ServicioLogistica(IPedidoRepository pedidos, IDespachoRepository despachos, IClienteRepository clientes,
        IEventoOutboxRepository outbox, IUnitOfWork uow, CatalogoFabricasEntrega entregas, IContextoUsuario usuario,
        IReloj reloj, EjecutorOperaciones ejecutor)
    {
        _pedidos = pedidos;
        _despachos = despachos;
        _clientes = clientes;
        _outbox = outbox;
        _uow = uow;
        _entregas = entregas;
        _usuario = usuario;
        _reloj = reloj;
        _ejecutor = ejecutor;
    }

    public Task<ResultadoOperacion> DespacharAsync(int pedidoId)
        => _ejecutor.EjecutarAsync("Despachar pedido", async () =>
        {
            PoliticaPermisos.Exigir(_usuario, Operacion.DespacharPedido);
            var pedido = await _pedidos.ObtenerPorIdAsync(pedidoId)
                ?? throw new RecursoNoEncontradoException($"No existe el pedido #{pedidoId}.");
            var cliente = await _clientes.ObtenerPorIdAsync(pedido.ClienteId);
            var ahora = _reloj.Ahora;
            var fabrica = _entregas.Obtener(pedido.TipoEntrega);

            pedido.Despachar();
            var despacho = Despacho.Registrar(pedido, fabrica, ahora);
            await _despachos.AgregarAsync(despacho);
            await _outbox.AgregarAsync(new EventoOutbox(EventoDespachoRegistrado,
                JsonSerializer.Serialize(new { pedido.Id, pedido.ClienteId, despacho.NumeroGuia, despacho.FechaEstimada }), ahora));
            await _uow.GuardarCambiosAsync();

            var etiqueta = fabrica.CrearEtiqueta().Construir(despacho.NumeroGuia, cliente?.Nombre ?? "Cliente", despacho.FechaEstimada);
            return $"Pedido #{pedidoId} despachado. {etiqueta}";
        });

    public Task<ResultadoOperacion> EntregarAsync(int pedidoId)
        => _ejecutor.EjecutarAsync("Registrar entrega", async () =>
        {
            PoliticaPermisos.Exigir(_usuario, Operacion.EntregarPedido);
            var pedido = await _pedidos.ObtenerPorIdAsync(pedidoId)
                ?? throw new RecursoNoEncontradoException($"No existe el pedido #{pedidoId}.");
            var despacho = await _despachos.ObtenerPorPedidoAsync(pedidoId)
                ?? throw new RecursoNoEncontradoException($"El pedido #{pedidoId} no tiene despacho registrado.");
            var ahora = _reloj.Ahora;

            pedido.Entregar(ahora);
            despacho.RegistrarEntrega(ahora);
            await _uow.GuardarCambiosAsync();
            return $"Pedido #{pedidoId} entregado el {ahora:dd/MM/yyyy HH:mm}.";
        });

    public async Task<Despacho?> ConsultarSeguimientoAsync(int pedidoId)
    {
        PoliticaPermisos.Exigir(_usuario, Operacion.ConsultarInformacion);
        return await _despachos.ObtenerPorPedidoAsync(pedidoId);
    }
}
