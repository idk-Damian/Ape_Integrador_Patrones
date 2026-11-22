namespace LogiMarket.Domain.Pedidos.Descuentos;

public sealed class SinDescuento : IEstrategiaDescuento
{
    public string Nombre => "NORMAL";
    public int Prioridad => 0;

    public bool Aplica(ContextoDescuento contexto) => true;

    public decimal CalcularDescuento(decimal subtotal) => 0m;
}
