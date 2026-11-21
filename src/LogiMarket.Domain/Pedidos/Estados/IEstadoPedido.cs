using LogiMarket.Domain.Pedidos;

namespace LogiMarket.Domain.Pedidos.Estados;

/// <summary>
/// Patrón STATE – Interfaz polimórfica para los estados del pedido.
/// Cada estado decide qué transiciones son válidas.
/// </summary>
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
