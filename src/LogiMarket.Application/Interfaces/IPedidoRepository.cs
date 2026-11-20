using LogiMarket.Domain.Pedidos;

namespace LogiMarket.Application.Interfaces;

public interface IPedidoRepository : IRepository<Pedido>
{
    Task<Pedido?> ObtenerConLineasAsync(int id);
    Task<IReadOnlyList<Pedido>> ObtenerPorClienteAsync(int clienteId);
}
