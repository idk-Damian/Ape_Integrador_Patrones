namespace LogiMarket.Application.Interfaces;

// unidad de trabajo: guarda los cambios del contexto y abre transacciones reales
public interface IUnitOfWork
{
    Task GuardarCambiosAsync();
    Task<ITransaccion> IniciarTransaccionAsync();
    void DescartarCambios();
}

public interface ITransaccion : IAsyncDisposable
{
    Task ConfirmarAsync();
    Task RevertirAsync();
}
