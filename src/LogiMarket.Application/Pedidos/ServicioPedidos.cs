using LogiMarket.Application.Comun;
using LogiMarket.Application.Entregas;
using LogiMarket.Application.Excepciones;
using LogiMarket.Application.Interfaces;
using LogiMarket.Application.Pedidos.Validacion;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Pedidos;
using LogiMarket.Domain.Pedidos.Descuentos;

namespace LogiMarket.Application.Pedidos;

public sealed record LineaSolicitada(string Codigo, int Cantidad);

// casos de uso del ciclo previo al cobro: crear borrador, validar y cancelar
public sealed class ServicioPedidos
{
    private readonly IPedidoRepository _pedidos;
    private readonly IClienteRepository _clientes;
    private readonly ResolutorCatalogo _catalogo;
    private readonly CatalogoFabricasEntrega _entregas;
    private readonly CalculadoraDescuento _calculadora;
    private readonly ServicioValidacionPedidos _validacion;
    private readonly IUnitOfWork _uow;
    private readonly IContextoUsuario _usuario;
    private readonly IReloj _reloj;
    private readonly EjecutorOperaciones _ejecutor;

    public ServicioPedidos(IPedidoRepository pedidos, IClienteRepository clientes, ResolutorCatalogo catalogo,
        CatalogoFabricasEntrega entregas, CalculadoraDescuento calculadora, ServicioValidacionPedidos validacion,
        IUnitOfWork uow, IContextoUsuario usuario, IReloj reloj, EjecutorOperaciones ejecutor)
    {
        _pedidos = pedidos;
        _clientes = clientes;
        _catalogo = catalogo;
        _entregas = entregas;
        _calculadora = calculadora;
        _validacion = validacion;
        _uow = uow;
        _usuario = usuario;
        _reloj = reloj;
        _ejecutor = ejecutor;
    }

    public async Task<int> CrearBorradorAsync(int clienteId, IReadOnlyList<LineaSolicitada> lineas, TipoEntrega entrega)
    {
        PoliticaPermisos.Exigir(_usuario, Operacion.GestionarPedidos);
        var cliente = await _clientes.ObtenerPorIdAsync(clienteId)
            ?? throw new RecursoNoEncontradoException($"No existe el cliente {clienteId}.");

        var builder = new PedidoBuilder(_calculadora)
            .ParaCliente(cliente)
            .ConEntrega(_entregas.Obtener(entrega))
            .EnFecha(_reloj.Ahora);
        foreach (var linea in lineas)
            builder.ConLinea(await _catalogo.ObtenerAsync(linea.Codigo), linea.Cantidad);

        var pedido = builder.Construir();
        await _pedidos.AgregarAsync(pedido);
        await _uow.GuardarCambiosAsync();
        return pedido.Id;
    }

    public async Task<ResultadoValidacion> ValidarAsync(int pedidoId)
    {
        var pedido = await ObtenerAsync(pedidoId);
        var resultado = await _validacion.ValidarAsync(pedido, Operacion.ValidarPedido);
        if (!resultado.Valido) return resultado;

        pedido.Validar(_reloj.Ahora);
        await _uow.GuardarCambiosAsync();
        return resultado;
    }

    public Task<ResultadoOperacion> CancelarAsync(int pedidoId)
        => _ejecutor.EjecutarAsync("Cancelar pedido", async () =>
        {
            PoliticaPermisos.Exigir(_usuario, Operacion.CancelarPedido);
            var pedido = await ObtenerAsync(pedidoId);
            pedido.Cancelar(_reloj.Ahora);
            await _uow.GuardarCambiosAsync();
            return $"Pedido #{pedidoId} cancelado.";
        });

    private async Task<Pedido> ObtenerAsync(int pedidoId)
        => await _pedidos.ObtenerPorIdAsync(pedidoId)
           ?? throw new RecursoNoEncontradoException($"No existe el pedido #{pedidoId}.");
}
