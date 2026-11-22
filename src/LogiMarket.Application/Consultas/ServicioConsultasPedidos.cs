using LogiMarket.Application.Consultas.Filtros;
using LogiMarket.Application.Seguridad;

namespace LogiMarket.Application.Consultas;

// el filtro se interpreta en memoria sobre la proyeccion: nunca se arma SQL con el texto del usuario
public sealed class ServicioConsultasPedidos
{
    private readonly IConsultasPedidos _consultas;
    private readonly IContextoUsuario _usuario;

    public ServicioConsultasPedidos(IConsultasPedidos consultas, IContextoUsuario usuario)
    {
        _consultas = consultas;
        _usuario = usuario;
    }

    public async Task<IReadOnlyList<PedidoResumen>> FiltrarAsync(string? filtro)
    {
        PoliticaPermisos.Exigir(_usuario, Operacion.ConsultarInformacion);
        var pedidos = await _consultas.ListarResumenAsync();
        if (string.IsNullOrWhiteSpace(filtro)) return pedidos;

        var expresion = ParserFiltro.Compilar(filtro);
        return pedidos.Where(expresion.Evaluar).ToList();
    }
}
