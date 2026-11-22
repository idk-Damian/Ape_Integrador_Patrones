namespace LogiMarket.Infrastructure.Pagos.Legado;

// componente heredado de otro sistema; se usa tal cual, sin cambiar su firma.
// trabaja en centavos y responde con un texto "OK;autorizacion" o "ERR;motivo"
public class PasarelaLegadaSimulada
{
    private readonly Dictionary<string, string> _respuestasPorReferencia = new();
    private readonly object _candado = new();

    public bool RechazarTodo { get; set; }
    public int CobrosProcesados { get; private set; }

    public string CobrarCentavos(long centavos, string referencia)
    {
        lock (_candado)
        {
            if (_respuestasPorReferencia.TryGetValue(referencia, out var anterior))
                return anterior;

            CobrosProcesados++;
            string respuesta;
            if (centavos <= 0) respuesta = "ERR;MONTO_INVALIDO";
            else if (RechazarTodo) respuesta = "ERR;TARJETA_RECHAZADA";
            else respuesta = "OK;LEG-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();

            if (respuesta.StartsWith("OK")) _respuestasPorReferencia[referencia] = respuesta;
            return respuesta;
        }
    }
}
