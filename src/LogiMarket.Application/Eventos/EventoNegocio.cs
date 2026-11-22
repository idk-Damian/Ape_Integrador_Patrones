using System.Text.Json;

namespace LogiMarket.Application.Eventos;

public static class TiposEvento
{
    public const string PedidoPagado = "PedidoPagado";
    public const string DespachoRegistrado = "DespachoRegistrado";
    public const string DevolucionAceptada = "DevolucionAceptada";
}

public sealed record EventoNegocio(Guid Id, string Tipo, string Payload, DateTime Fecha)
{
    public int Entero(string propiedad) => Leer(propiedad).GetInt32();
    public decimal Decimal(string propiedad) => Leer(propiedad).GetDecimal();
    public string Texto(string propiedad) => Leer(propiedad).ToString();

    private JsonElement Leer(string propiedad)
    {
        using var doc = JsonDocument.Parse(Payload);
        return doc.RootElement.GetProperty(propiedad).Clone();
    }
}
