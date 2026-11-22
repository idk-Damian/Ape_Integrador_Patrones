using LogiMarket.Domain.Common;
using LogiMarket.Domain.Enums;

namespace LogiMarket.Domain.Pedidos.Descuentos;

// ya no repite la regla de mayorista: si ambas aplican gana la de mayor prioridad
public sealed class Vip10 : IEstrategiaDescuento
{
    public string Nombre => "VIP";
    public int Prioridad => 50;

    public bool Aplica(ContextoDescuento contexto) => contexto.Categoria == CategoriaCliente.Vip;

    public decimal CalcularDescuento(decimal subtotal) => Moneda.Redondear(subtotal * 0.10m);
}
