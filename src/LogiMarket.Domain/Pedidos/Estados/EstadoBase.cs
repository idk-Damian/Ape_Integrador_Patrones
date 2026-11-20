using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Pedidos.Estados;

/// <summary>
/// Base abstracta para estados del pedido.
/// Cada método lanza DomainException por defecto; los estados concretos
/// solo sobreescriben las transiciones válidas.
/// </summary>
public abstract class EstadoBase : IEstadoPedido
{
    public abstract string Nombre { get; }
    public abstract bool PuedeEditar { get; }

    public virtual void Validar(Pedido pedido)
        => throw new DomainException($"No se puede validar un pedido en estado '{Nombre}'.");

    public virtual void Pagar(Pedido pedido)
        => throw new DomainException($"No se puede pagar un pedido en estado '{Nombre}'.");

    public virtual void Despachar(Pedido pedido)
        => throw new DomainException($"No se puede despachar un pedido en estado '{Nombre}'.");

    public virtual void Entregar(Pedido pedido)
        => throw new DomainException($"No se puede entregar un pedido en estado '{Nombre}'.");

    public virtual void Cancelar(Pedido pedido)
        => throw new DomainException($"No se puede cancelar un pedido en estado '{Nombre}'.");
}
