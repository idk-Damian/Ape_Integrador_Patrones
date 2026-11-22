using LogiMarket.Application.Comun;
using LogiMarket.Application.Devoluciones;
using LogiMarket.Application.Logistica;
using LogiMarket.Application.Pedidos;
using LogiMarket.Application.Pedidos.Confirmacion;

namespace LogiMarket.Application.Comandos;

// cada accion de negocio empaquetada como objeto; boton y prueba ejecutan el mismo comando
public interface IComando
{
    string Nombre { get; }
    string Entrada { get; }
    Task<ResultadoOperacion> EjecutarAsync();
}

public sealed class ConfirmarPedidoCommand : IComando
{
    private readonly ConfirmacionPedidoFacade _facade;
    private readonly int _pedidoId;
    private readonly string _clave;

    public ConfirmarPedidoCommand(ConfirmacionPedidoFacade facade, int pedidoId, string clave)
    {
        _facade = facade;
        _pedidoId = pedidoId;
        _clave = clave;
    }

    public string Nombre => "ConfirmarPedido";
    public string Entrada => $"pedido={_pedidoId}; clave={_clave}";
    public ResultadoConfirmacion? Resultado { get; private set; }

    public async Task<ResultadoOperacion> EjecutarAsync()
    {
        Resultado = await _facade.ConfirmarAsync(_pedidoId, _clave);
        return new ResultadoOperacion(Resultado.Exito, Resultado.Mensaje);
    }
}

public sealed class CancelarPedidoCommand : IComando
{
    private readonly ServicioPedidos _pedidos;
    private readonly int _pedidoId;

    public CancelarPedidoCommand(ServicioPedidos pedidos, int pedidoId)
    {
        _pedidos = pedidos;
        _pedidoId = pedidoId;
    }

    public string Nombre => "CancelarPedido";
    public string Entrada => $"pedido={_pedidoId}";
    public Task<ResultadoOperacion> EjecutarAsync() => _pedidos.CancelarAsync(_pedidoId);
}

public sealed class DespacharPedidoCommand : IComando
{
    private readonly ServicioLogistica _logistica;
    private readonly int _pedidoId;

    public DespacharPedidoCommand(ServicioLogistica logistica, int pedidoId)
    {
        _logistica = logistica;
        _pedidoId = pedidoId;
    }

    public string Nombre => "DespacharPedido";
    public string Entrada => $"pedido={_pedidoId}";
    public Task<ResultadoOperacion> EjecutarAsync() => _logistica.DespacharAsync(_pedidoId);
}

public sealed class EntregarPedidoCommand : IComando
{
    private readonly ServicioLogistica _logistica;
    private readonly int _pedidoId;

    public EntregarPedidoCommand(ServicioLogistica logistica, int pedidoId)
    {
        _logistica = logistica;
        _pedidoId = pedidoId;
    }

    public string Nombre => "EntregarPedido";
    public string Entrada => $"pedido={_pedidoId}";
    public Task<ResultadoOperacion> EjecutarAsync() => _logistica.EntregarAsync(_pedidoId);
}

public sealed class ProcesarDevolucionCommand : IComando
{
    private readonly ServicioDevoluciones _devoluciones;
    private readonly SolicitudDevolucion _solicitud;

    public ProcesarDevolucionCommand(ServicioDevoluciones devoluciones, SolicitudDevolucion solicitud)
    {
        _devoluciones = devoluciones;
        _solicitud = solicitud;
    }

    public string Nombre => "ProcesarDevolucion";
    public string Entrada => $"pedido={_solicitud.PedidoId}; motivo={_solicitud.Motivo}; sinUso={_solicitud.ProductoSinUso}";
    public ResultadoDevolucion? Resultado { get; private set; }

    public async Task<ResultadoOperacion> EjecutarAsync()
    {
        Resultado = await _devoluciones.ProcesarAsync(_solicitud);
        return new ResultadoOperacion(Resultado.Aceptada, Resultado.Mensaje);
    }
}
