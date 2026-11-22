using LogiMarket.Application.Interfaces;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LogiMarket.Domain.Clientes;

namespace LogiMarket.Infrastructure.Repositories;

public sealed class ClienteRepository : IClienteRepository
{
    private readonly LogiMarketDbContext _context;

    public ClienteRepository(LogiMarketDbContext context) => _context = context;

    public Task<Cliente?> ObtenerPorIdAsync(int id) => _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IReadOnlyList<Cliente>> ListarAsync(string? texto = null)
    {
        var consulta = _context.Clientes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(texto))
        {
            var filtro = texto.Trim().ToLower();
            consulta = consulta.Where(c => c.Nombre.ToLower().Contains(filtro) || c.Email.Contains(filtro));
        }
        return await consulta.OrderBy(c => c.Nombre).ToListAsync();
    }

    public Task<bool> ExisteEmailAsync(string email, int? excluirId = null)
    {
        var buscado = (email ?? string.Empty).Trim().ToLowerInvariant();
        return _context.Clientes.AnyAsync(c => c.Email == buscado && (excluirId == null || c.Id != excluirId));
    }

    public async Task AgregarAsync(Cliente cliente) => await _context.Clientes.AddAsync(cliente);
}
