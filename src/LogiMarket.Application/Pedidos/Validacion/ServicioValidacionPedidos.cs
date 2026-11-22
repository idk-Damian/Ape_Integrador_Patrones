using LogiMarket.Application.Interfaces;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Pedidos;

namespace LogiMarket.Application.Pedidos.Validacion;

// arma la cadena en orden fijo y le entrega los datos ya cargados
public sealed class ServicioValidacionPedidos
{
    private readonly IClienteRepository _clientes;
    private readonly IProductoRepository _productos;
    private readonly IContextoUsuario _usuario;

    public ServicioValidacionPedidos(IClienteRepository clientes, IProductoRepository productos, IContextoUsuario usuario)
    {
        _clientes = clientes;
        _productos = productos;
        _usuario = usuario;
    }

    public static ValidadorPedido CrearCadena()
    {
        var inicio = new ValidadorClienteActivo();
        inicio.Enlazar(new ValidadorLineas())
              .Enlazar(new ValidadorStock())
              .Enlazar(new ValidadorAutorizacion());
        return inicio;
    }

    public async Task<ResultadoValidacion> ValidarAsync(Pedido pedido, Operacion operacion)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(pedido.ClienteId);
        var productos = (await _productos.ObtenerPorCodigosAsync(pedido.RequerimientoPorSku().Keys))
            .ToDictionary(p => p.Codigo);

        return CrearCadena().Validar(new ContextoValidacion(pedido, cliente, productos, _usuario, operacion));
    }
}
