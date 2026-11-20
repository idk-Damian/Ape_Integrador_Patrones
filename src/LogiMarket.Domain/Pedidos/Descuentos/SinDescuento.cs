using LogiMarket.Domain.Enums;

namespace LogiMarket.Domain.Pedidos.Descuentos;

/// <summary>NORMAL: 0% de descuento. Prioridad más baja.</summary>
public sealed class SinDescuento : IEstrategiaDescuento
{
    public string Nombre => "NORMAL";
    public int Prioridad => 0;

    public bool Aplica(int totalUnidadesFisicas, CategoriaCliente categoriaCliente)
        => true;

    public decimal CalcularDescuento(decimal subtotal) => 0m;
}
