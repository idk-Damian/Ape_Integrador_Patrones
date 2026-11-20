using System.Linq.Expressions;

namespace LogiMarket.Application.Interfaces;

/// <summary>Repositorio genérico base.</summary>
public interface IRepository<T> where T : class
{
    Task<T?> ObtenerPorIdAsync(int id);
    Task<IReadOnlyList<T>> ObtenerTodosAsync();
    Task<IReadOnlyList<T>> BuscarAsync(Expression<Func<T, bool>> predicado);
    Task AgregarAsync(T entidad);
    void Actualizar(T entidad);
    void Eliminar(T entidad);
}
