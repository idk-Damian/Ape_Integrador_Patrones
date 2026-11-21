using LogiMarket.Domain.Clientes;

namespace LogiMarket.Application.Interfaces;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente?> ObtenerPorEmailAsync(string email);
}
