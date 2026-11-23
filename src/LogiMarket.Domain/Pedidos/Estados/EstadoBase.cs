using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Pedidos.Estados;

// por defecto toda accion es invalida; cada estado concreto habilita solo las suyas
public abstract class EstadoBase : IEstadoPedido
{
    public abstract string Nombre { get; }
    public abstract bool PuedeEditar { get; }

    public virtual void Validar(Pedido pedido, DateTime fecha) => Rechazar("validar");
    public virtual void Pagar(Pedido pedido, DateTime fecha) => Rechazar("pagar");
    public virtual void Despachar(Pedido pedido, DateTime fecha) => Rechazar("despachar");
    public virtual void Entregar(Pedido pedido, DateTime fecha) => Rechazar("entregar");
    public virtual void Cancelar(Pedido pedido, DateTime fecha) => Rechazar("cancelar");

    private void Rechazar(string accion)
        => throw new DomainException($"No se puede {accion} un pedido en estado '{Nombre}'.");
}
