using System.Linq.Expressions;
using LogiMarket.Application.Interfaces;
using LogiMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly LogiMarketDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(LogiMarketDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> ObtenerPorIdAsync(int id)
        => await DbSet.FindAsync(id);

    public virtual async Task<IReadOnlyList<T>> ObtenerTodosAsync()
        => await DbSet.ToListAsync();

    public virtual async Task<IReadOnlyList<T>> BuscarAsync(Expression<Func<T, bool>> predicado)
        => await DbSet.Where(predicado).ToListAsync();

    public virtual async Task AgregarAsync(T entidad)
        => await DbSet.AddAsync(entidad);

    public virtual void Actualizar(T entidad)
        => DbSet.Update(entidad);

    public virtual void Eliminar(T entidad)
        => DbSet.Remove(entidad);
}
