namespace LogiMarket.Domain.Pedidos.Descuentos;

// cada politica dice si aplica y cuanto descuenta; la prioridad decide cual gana
public interface IEstrategiaDescuento
{
    string Nombre { get; }
    int Prioridad { get; }
    bool Aplica(ContextoDescuento contexto);
    decimal CalcularDescuento(decimal subtotal);
}
