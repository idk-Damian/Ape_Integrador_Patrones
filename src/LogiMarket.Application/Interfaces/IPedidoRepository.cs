using LogiMarket.Domain.Pedidos;

namespace LogiMarket.Application.Interfaces;

public interface IPedidoRepository
{
    Task<Pedido?> ObtenerPorIdAsync(int id);
    Task<IReadOnlyList<Pedido>> ListarAsync();
    Task AgregarAsync(Pedido pedido);
}
