namespace LogiMarket.Domain.Pedidos.Estados;

public sealed class EstadoPagado : EstadoBase
{
    public static readonly EstadoPagado Instancia = new();
    public override string Nombre => "PAGADO";
    public override bool PuedeEditar => false;

    public override void Despachar(Pedido pedido, DateTime fecha)
        => pedido.CambiarEstado(EstadoDespachado.Instancia, fecha);
}
