namespace LogiMarket.Domain.Pedidos.Estados;

/// <summary>CANCELADO – Estado terminal. Ninguna transición permitida.</summary>
public sealed class EstadoCancelado : EstadoBase
{
    public static readonly EstadoCancelado Instancia = new();
    public override string Nombre => "CANCELADO";
    public override bool PuedeEditar => false;
}
