using LogiMarket.Domain.Clientes;

namespace LogiMarket.Application.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task<IReadOnlyList<Cliente>> ListarAsync(string? texto = null);
    Task<bool> ExisteEmailAsync(string email, int? excluirId = null);
    Task AgregarAsync(Cliente cliente);
}
