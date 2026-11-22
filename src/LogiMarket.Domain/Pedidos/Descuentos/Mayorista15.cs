using LogiMarket.Domain.Common;

namespace LogiMarket.Domain.Pedidos.Descuentos;

public sealed class Mayorista15 : IEstrategiaDescuento
{
    public string Nombre => "MAYORISTA";
    public int Prioridad => 100;

    public bool Aplica(ContextoDescuento contexto) => contexto.UnidadesFisicas >= 10;

    public decimal CalcularDescuento(decimal subtotal) => Moneda.Redondear(subtotal * 0.15m);
}
