using LogiMarket.Application.Excepciones;
using LogiMarket.Application.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LogiMarket.Infrastructure.Data;

// el contexto lo crea y lo libera el contenedor; esta clase no lo cierra
public sealed class UnitOfWork : IUnitOfWork
{
    private const int SqliteRestriccionUnica = 2067;
    private const int SqliteLlavePrimaria = 1555;
    private const int SqliteOcupada = 5;
    private const int SqliteBloqueada = 6;

    private readonly LogiMarketDbContext _context;

    public UnitOfWork(LogiMarketDbContext context) => _context = context;

    public async Task GuardarCambiosAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictoConcurrenciaException(
                "Otro usuario modificó los mismos datos. Vuelva a cargar e intente de nuevo.", ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException
               { SqliteExtendedErrorCode: SqliteRestriccionUnica or SqliteLlavePrimaria })
        {
            throw new ConflictoDuplicadoException("Ya existe un registro con ese valor único.", ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: SqliteOcupada or SqliteBloqueada })
        {
            throw new ConflictoConcurrenciaException("La base está ocupada por otra operación; intente de nuevo.", ex);
        }
    }

    public async Task<ITransaccion> IniciarTransaccionAsync()
    {
        try
        {
            return new TransaccionEf(await _context.Database.BeginTransactionAsync());
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode is SqliteOcupada or SqliteBloqueada)
        {
            throw new ConflictoConcurrenciaException("La base está ocupada por otra operación; intente de nuevo.", ex);
        }
    }

    public void DescartarCambios() => _context.ChangeTracker.Clear();

    private sealed class TransaccionEf : ITransaccion
    {
        private readonly IDbContextTransaction _transaccion;
        private bool _cerrada;

        public TransaccionEf(IDbContextTransaction transaccion) => _transaccion = transaccion;

        public async Task ConfirmarAsync()
        {
            await _transaccion.CommitAsync();
            _cerrada = true;
        }

        public async Task RevertirAsync()
        {
            if (_cerrada) return;
            await _transaccion.RollbackAsync();
            _cerrada = true;
        }

        public async ValueTask DisposeAsync()
        {
            // si nadie confirmo, se revierte al salir del using
            if (!_cerrada) await _transaccion.RollbackAsync();
            await _transaccion.DisposeAsync();
        }
    }
}
