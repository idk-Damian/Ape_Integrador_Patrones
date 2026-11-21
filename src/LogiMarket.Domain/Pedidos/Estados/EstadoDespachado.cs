namespace LogiMarket.Domain.Pedidos.Estados;

/// <summary>DESPACHADO → puede Entregar. No permite edición ni retroceso.</summary>
public sealed class EstadoDespachado : EstadoBase
{
    public static readonly EstadoDespachado Instancia = new();
    public override string Nombre => "DESPACHADO";
    public override bool PuedeEditar => false;

    public override void Entregar(Pedido pedido)
        => pedido.CambiarEstado(EstadoEntregado.Instancia);
}
