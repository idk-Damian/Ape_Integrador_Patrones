namespace LogiMarket.Domain.Pedidos.Estados;

/// <summary>ENTREGADO – Estado terminal. Ninguna transición permitida.</summary>
public sealed class EstadoEntregado : EstadoBase
{
    public static readonly EstadoEntregado Instancia = new();
    public override string Nombre => "ENTREGADO";
    public override bool PuedeEditar => false;
}
