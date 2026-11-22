using LogiMarket.Domain.Enums;

namespace LogiMarket.Domain.Pedidos.Descuentos;

public sealed record ContextoDescuento(int UnidadesFisicas, CategoriaCliente Categoria, decimal Subtotal);

public sealed record ResultadoDescuento(string Politica, decimal Monto);
