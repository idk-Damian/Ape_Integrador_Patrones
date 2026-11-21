namespace LogiMarket.Domain.Pedidos.Descuentos;

/// <summary>
/// Patrón STRATEGY – Estrategia de descuento con prioridad para selección automática.
/// </summary>
public interface IEstrategiaDescuento
{
    string Nombre { get; }
    int Prioridad { get; }
    bool Aplica(int totalUnidadesFisicas, Enums.CategoriaCliente categoriaCliente);
    decimal CalcularDescuento(decimal subtotal);
}
