namespace LogiMarket.Domain.Pedidos.Estados;

public sealed class EstadoDespachado : EstadoBase
{
    public static readonly EstadoDespachado Instancia = new();
    public override string Nombre => "DESPACHADO";
    public override bool PuedeEditar => false;

    public override void Entregar(Pedido pedido, DateTime fecha)
        => pedido.CambiarEstado(EstadoEntregado.Instancia, fecha);
}
