namespace LogiMarket.Domain.Pedidos.Estados;

public interface IEstadoPedido
{
    string Nombre { get; }
    bool PuedeEditar { get; }

    void Validar(Pedido pedido);
    void Pagar(Pedido pedido);
    void Despachar(Pedido pedido);
    void Entregar(Pedido pedido);
    void Cancelar(Pedido pedido);
}
