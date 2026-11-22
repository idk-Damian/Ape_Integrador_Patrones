namespace LogiMarket.Domain.Pedidos.Estados;

public interface IEstadoPedido
{
    string Nombre { get; }
    bool PuedeEditar { get; }

    // la fecha llega desde Application (IReloj); el dominio no consulta la hora del sistema
    void Validar(Pedido pedido, DateTime fecha);
    void Pagar(Pedido pedido, DateTime fecha);
    void Despachar(Pedido pedido, DateTime fecha);
    void Entregar(Pedido pedido, DateTime fecha);
    void Cancelar(Pedido pedido, DateTime fecha);
}
