namespace LogiMarket.Domain.Pedidos.Estados;

/// <summary>PAGADO → puede Despachar. Prohibido cancelar, editar o retroceder.</summary>
public sealed class EstadoPagado : EstadoBase
{
    public static readonly EstadoPagado Instancia = new();
    public override string Nombre => "PAGADO";
    public override bool PuedeEditar => false;

    public override void Despachar(Pedido pedido)
        => pedido.CambiarEstado(EstadoDespachado.Instancia);
}
