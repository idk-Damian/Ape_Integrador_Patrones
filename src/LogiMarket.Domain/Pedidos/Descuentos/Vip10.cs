using LogiMarket.Domain.Common;
using LogiMarket.Domain.Enums;

namespace LogiMarket.Domain.Pedidos.Descuentos;

/// <summary>VIP: 10% si cliente es VIP y NO aplica MAYORISTA. Prioridad media.</summary>
public sealed class Vip10 : IEstrategiaDescuento
{
    public string Nombre => "VIP";
    public int Prioridad => 5;

    public bool Aplica(int totalUnidadesFisicas, CategoriaCliente categoriaCliente)
        => categoriaCliente == CategoriaCliente.Vip && totalUnidadesFisicas < 10;

    public decimal CalcularDescuento(decimal subtotal)
        => Moneda.Redondear(subtotal * 0.10m);
}
