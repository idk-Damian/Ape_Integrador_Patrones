namespace LogiMarket.Domain.Pedidos.Estados;

public sealed class EstadoValidado : EstadoBase
{
    public static readonly EstadoValidado Instancia = new();
    public override string Nombre => "VALIDADO";
    public override bool PuedeEditar => false;

    public override void Pagar(Pedido pedido) => pedido.CambiarEstado(EstadoPagado.Instancia);

    public override void Cancelar(Pedido pedido) => pedido.CambiarEstado(EstadoCancelado.Instancia);
}
