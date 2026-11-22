using LogiMarket.Application.Excepciones;
using LogiMarket.Application.Interfaces;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Application.Comun;

// un solo lugar para convertir errores esperados en respuestas y descartar cambios a medias
public sealed class EjecutorOperaciones
{
    private readonly IUnitOfWork _uow;
    private readonly IBitacora _bitacora;

    public EjecutorOperaciones(IUnitOfWork uow, IBitacora bitacora)
    {
        _uow = uow;
        _bitacora = bitacora;
    }

    public async Task<ResultadoOperacion> EjecutarAsync(string operacion, Func<Task<string>> accion)
    {
        try
        {
            return ResultadoOperacion.Ok(await accion());
        }
        catch (Exception ex) when (EsEsperada(ex))
        {
            _uow.DescartarCambios();
            var categoria = ex is AccesoDenegadoException ? CategoriasBitacora.Seguridad : CategoriasBitacora.Negocio;
            _bitacora.Registrar(categoria, $"{operacion} rechazada: {ex.Message}");
            return ResultadoOperacion.Falla(ex.Message);
        }
    }

    public static bool EsEsperada(Exception ex)
        => ex is DomainException or AccesoDenegadoException or ConflictoConcurrenciaException
            or ConflictoDuplicadoException or RecursoNoEncontradoException;
}
