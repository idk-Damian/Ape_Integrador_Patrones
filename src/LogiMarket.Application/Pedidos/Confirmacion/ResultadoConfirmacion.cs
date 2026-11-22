using LogiMarket.Application.Pedidos.Validacion;

namespace LogiMarket.Application.Pedidos.Confirmacion;

public sealed record ResultadoConfirmacion(
    bool Exito,
    int PedidoId,
    string Mensaje,
    decimal Total = 0m,
    string? ReferenciaCobro = null,
    bool EsRepeticion = false,
    ResultadoValidacion? Validacion = null);
