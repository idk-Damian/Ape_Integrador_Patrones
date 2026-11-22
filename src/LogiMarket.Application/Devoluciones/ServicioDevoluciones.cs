using LogiMarket.Application.Comun;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Application.Devoluciones;

public sealed class ServicioDevoluciones
{
    private readonly IReadOnlyDictionary<MotivoDevolucion, ProcesadorDevolucionBase> _procesadores;

    public ServicioDevoluciones(IEnumerable<ProcesadorDevolucionBase> procesadores)
        => _procesadores = procesadores.ToDictionary(p => p.Motivo);

    public async Task<ResultadoDevolucion> ProcesarAsync(SolicitudDevolucion solicitud)
    {
        if (!_procesadores.TryGetValue(solicitud.Motivo, out var procesador))
            throw new DomainException($"No hay procesador para el motivo {solicitud.Motivo}.");

        try
        {
            return await procesador.ProcesarAsync(solicitud);
        }
        catch (Exception ex) when (EjecutorOperaciones.EsEsperada(ex))
        {
            return new ResultadoDevolucion(false, ex.Message, 0m, 0, Array.Empty<string>());
        }
    }
}
