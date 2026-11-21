using LogiMarket.Application.Interfaces;
using LogiMarket.Domain.Clientes;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Repositories;

public class ClienteRepository : Repository<Cliente>, IClienteRepository
{
    public ClienteRepository(LogiMarketDbContext context) : base(context) { }

    public async Task<Cliente?> ObtenerPorEmailAsync(string email)
        => await DbSet.FirstOrDefaultAsync(c => c.Email == email);
}
