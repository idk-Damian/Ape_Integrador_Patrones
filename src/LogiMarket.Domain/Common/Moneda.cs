namespace LogiMarket.Domain.Common;

public static class Moneda
{
    public static decimal Redondear(decimal valor)
        => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
