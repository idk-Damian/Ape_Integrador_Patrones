using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Clientes;
using LogiMarket.Domain.Pedidos;

namespace LogiMarket.Application.Pedidos.Validacion;

public sealed class ContextoValidacion
{
    public ContextoValidacion(Pedido pedido, Cliente? cliente, IReadOnlyDictionary<string, ProductoSimple> productos,
        IContextoUsuario usuario, Operacion operacion)
    {
        Pedido = pedido;
        Cliente = cliente;
        Productos = productos;
        Usuario = usuario;
        Operacion = operacion;
    }

    public Pedido Pedido { get; }
    public Cliente? Cliente { get; }
    public IReadOnlyDictionary<string, ProductoSimple> Productos { get; }
    public IContextoUsuario Usuario { get; }
    public Operacion Operacion { get; }
}

public sealed record ResultadoValidacion(bool Valido, string? EslabonFallido, string? Motivo, IReadOnlyList<string> Recorrido);
