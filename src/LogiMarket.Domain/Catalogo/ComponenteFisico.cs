namespace LogiMarket.Domain.Catalogo;

/// <summary>
/// Representa un componente físico resuelto (SKU final) con cantidad propagada.
/// </summary>
public sealed record ComponenteFisico(
    string Codigo,
    string Nombre,
    int Cantidad,
    decimal PrecioUnitario,
    decimal PesoKg);
