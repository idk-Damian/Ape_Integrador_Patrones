using LogiMarket.Domain.Outbox;

namespace LogiMarket.Application.Interfaces;

// bitacora de solo escritura y lectura
public interface IAuditoriaRepository
{
    Task RegistrarAsync(Auditoria registro);
    Task<bool> ExisteAsync(string categoria, string referencia);
    Task<IReadOnlyList<Auditoria>> ListarAsync(string? categoria = null);
}
