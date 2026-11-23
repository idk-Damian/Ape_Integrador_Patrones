namespace LogiMarket.Domain.Pedidos.Estados;

public sealed class EstadoValidado : EstadoBase
{
    public static readonly EstadoValidado Instancia = new();
    public override string Nombre => "VALIDADO";
    public override bool PuedeEditar => false;

    public override void Pagar(Pedido pedido, DateTime fecha) => pedido.CambiarEstado(EstadoPagado.Instancia, fecha);

    public override void Cancelar(Pedido pedido, DateTime fecha) => pedido.CambiarEstado(EstadoCancelado.Instancia, fecha);
}
