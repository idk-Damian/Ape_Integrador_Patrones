namespace LogiMarket.Domain.Pedidos.Estados;

public sealed class EstadoBorrador : EstadoBase
{
    public static readonly EstadoBorrador Instancia = new();
    public override string Nombre => "BORRADOR";
    public override bool PuedeEditar => true;

    public override void Validar(Pedido pedido, DateTime fecha)
        => pedido.CambiarEstado(EstadoValidado.Instancia, fecha);

    public override void Cancelar(Pedido pedido, DateTime fecha)
        => pedido.CambiarEstado(EstadoCancelado.Instancia, fecha);
}
