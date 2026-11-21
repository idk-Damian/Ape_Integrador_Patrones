namespace LogiMarket.Domain.Pedidos.Estados;

/// <summary>VALIDADO → puede Pagar o Cancelar. No permite edición.</summary>
public sealed class EstadoValidado : EstadoBase
{
    public static readonly EstadoValidado Instancia = new();
    public override string Nombre => "VALIDADO";
    public override bool PuedeEditar => false;

    public override void Pagar(Pedido pedido)
    {
        pedido.CambiarEstado(EstadoPagado.Instancia);
        pedido.BloquearHistorial();
    }

    public override void Cancelar(Pedido pedido)
        => pedido.CambiarEstado(EstadoCancelado.Instancia);
}
