using LogiMarket.Domain.Outbox;

namespace LogiMarket.Application.Interfaces;

public interface IAuditoriaRepository : IRepository<Auditoria>
{
    Task<IReadOnlyList<Auditoria>> ObtenerPorEntidadAsync(string entidad, int entidadId);
}
