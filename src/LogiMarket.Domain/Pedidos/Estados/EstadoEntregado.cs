namespace LogiMarket.Domain.Pedidos.Estados;

public sealed class EstadoEntregado : EstadoBase
{
    public static readonly EstadoEntregado Instancia = new();
    public override string Nombre => "ENTREGADO";
    public override bool PuedeEditar => false;
}
