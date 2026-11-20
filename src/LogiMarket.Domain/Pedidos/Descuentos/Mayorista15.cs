using LogiMarket.Domain.Common;
using LogiMarket.Domain.Enums;

namespace LogiMarket.Domain.Pedidos.Descuentos;

/// <summary>MAYORISTA: 15% si total unidades físicas >= 10. Prioridad más alta.</summary>
public sealed class Mayorista15 : IEstrategiaDescuento
{
    public string Nombre => "MAYORISTA";
    public int Prioridad => 10;

    public bool Aplica(int totalUnidadesFisicas, CategoriaCliente categoriaCliente)
        => totalUnidadesFisicas >= 10;

    public decimal CalcularDescuento(decimal subtotal)
        => Moneda.Redondear(subtotal * 0.15m);
}
