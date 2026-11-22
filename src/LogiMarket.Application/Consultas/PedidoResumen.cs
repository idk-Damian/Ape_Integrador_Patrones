using LogiMarket.Domain.Enums;

namespace LogiMarket.Application.Consultas;

public sealed record PedidoResumen(int Id, string Cliente, string Estado, decimal Total, DateTime Fecha, TipoEntrega Entrega);

// lado de consulta (CQRS basico): proyecciones de solo lectura sin cargar agregados completos
public interface IConsultasPedidos
{
    Task<IReadOnlyList<PedidoResumen>> ListarResumenAsync();
}
