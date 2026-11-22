using LogiMarket.Application.Interfaces;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LogiMarket.Domain.Usuarios;

namespace LogiMarket.Infrastructure.Repositories;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly LogiMarketDbContext _context;

    public UsuarioRepository(LogiMarketDbContext context) => _context = context;

    public Task<Usuario?> ObtenerPorNombreAsync(string nombreUsuario)
        => _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == (nombreUsuario ?? string.Empty).Trim().ToLower());

    public async Task AgregarAsync(Usuario usuario) => await _context.Usuarios.AddAsync(usuario);
}
