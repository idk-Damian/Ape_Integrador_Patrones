using LogiMarket.Application.Consultas;
using LogiMarket.Application.Consultas.Filtros;
using LogiMarket.Domain.Enums;
using LogiMarket.Tests.Soporte;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Tests.Aplicacion;

public class InterpreterTests
{
    private static readonly List<PedidoResumen> Pedidos = new()
    {
        new(1, "Ana Torres", "PAGADO", 59.00m, DateTime.Today, TipoEntrega.Estandar),
        new(2, "Luis Mena", "BORRADOR", 102.00m, DateTime.Today, TipoEntrega.Expres),
        new(3, "Carmen Ruiz", "PAGADO", 90.00m, DateTime.Today, TipoEntrega.Estandar),
        new(4, "Luis Mena", "PAGADO", 10.00m, DateTime.Today, TipoEntrega.Estandar),
        new(5, "Ana Torres", "ENTREGADO", 30.00m, DateTime.Today, TipoEntrega.Estandar)
    };

    private static int[] Filtrar(string filtro)
    {
        var expresion = ParserFiltro.Compilar(filtro);
        return Pedidos.Where(expresion.Evaluar).Select(p => p.Id).ToArray();
    }

    [Fact]
    public void P25_EstadoPagadoYTotalMinimo50()
        => Assert.Equal(new[] { 1, 3 }, Filtrar("ESTADO = PAGADO AND TOTAL >= 50"));

    [Fact]
    public void P26_AndLigaMasFuerteQueOrYLosParentesisCambianElResultado()
    {
        Assert.Equal(new[] { 1, 2, 3 }, Filtrar("ESTADO = BORRADOR OR ESTADO = PAGADO AND TOTAL >= 50"));
        Assert.Equal(new[] { 2, 3 }, Filtrar("(ESTADO = BORRADOR OR ESTADO = PAGADO) AND TOTAL >= 60"));
        Assert.Equal(new[] { 1, 5 }, Filtrar("cliente = \"Ana Torres\""));
        Assert.Equal(new[] { 2, 4 }, Filtrar("CLIENTE = luis"));
    }

    [Fact]
    public void P26_ArbolDeExpresionesRespetaLaPrecedencia()
        => Assert.Equal("(ESTADO = BORRADOR OR (ESTADO = PAGADO AND TOTAL >= 50))",
            ParserFiltro.Compilar("ESTADO = BORRADOR OR ESTADO = PAGADO AND TOTAL >= 50").Describir());

    [Theory]
    [InlineData("ESTADO = PAGADO AND", "terminó")]
    [InlineData("(ESTADO = PAGADO", "')'")]
    [InlineData("TOTAL >= abc", "número")]
    [InlineData("TOTAL > 50", ">=")]
    [InlineData("ESTADO = PERDIDO", "no es un estado")]
    [InlineData("PRECIO = 5", "Campo desconocido")]
    [InlineData("ESTADO = PAGADO; DROP TABLE Pedidos", "no permitido")]
    [InlineData("", "vacío")]
    public void P26_SintaxisInvalidaDaUnErrorLegible(string filtro, string fragmento)
    {
        var ex = Assert.Throws<FiltroInvalidoException>(() => ParserFiltro.Compilar(filtro));
        Assert.Contains(fragmento, ex.Message);
    }

    [Fact]
    public async Task P25_FiltroSobreDatosRealesDeLaBase()
    {
        using var e = new Entorno();
        var ana = await e.PagadoAsync("Ana", TipoEntrega.Estandar, ("KIT_TELETRABAJO", 1));
        await e.BorradorAsync("Luis", TipoEntrega.Expres, ("P04", 2), ("P05", 2));
        await e.PagadoAsync("Luis", TipoEntrega.Estandar, ("P05", 1));
        e.ComoRol(RolUsuario.Consulta);

        var resultado = await e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioConsultasPedidos>()
            .FiltrarAsync("ESTADO = PAGADO AND TOTAL >= 50"));

        Assert.Equal(new[] { ana }, resultado.Select(r => r.Id));
    }
}
