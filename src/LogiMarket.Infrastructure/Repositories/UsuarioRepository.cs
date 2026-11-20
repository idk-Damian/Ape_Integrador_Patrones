using LogiMarket.Application.Interfaces;
using LogiMarket.Domain.Usuarios;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Repositories;

public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(LogiMarketDbContext context) : base(context) { }

    public async Task<Usuario?> ObtenerPorNombreAsync(string nombreUsuario)
        => await DbSet.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
}
