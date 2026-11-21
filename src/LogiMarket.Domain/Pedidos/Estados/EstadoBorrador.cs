namespace LogiMarket.Domain.Pedidos.Estados;

/// <summary>BORRADOR → puede Validar o Cancelar. Permite edición.</summary>
public sealed class EstadoBorrador : EstadoBase
{
    public static readonly EstadoBorrador Instancia = new();
    public override string Nombre => "BORRADOR";
    public override bool PuedeEditar => true;

    public override void Validar(Pedido pedido)
        => pedido.CambiarEstado(EstadoValidado.Instancia);

    public override void Cancelar(Pedido pedido)
        => pedido.CambiarEstado(EstadoCancelado.Instancia);
}
