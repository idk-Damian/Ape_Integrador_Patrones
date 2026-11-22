using LogiMarket.Application.Comun;
using LogiMarket.Application.Excepciones;
using LogiMarket.Application.Interfaces;
using LogiMarket.Application.Pagos;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Pedidos;
using LogiMarket.Infrastructure.Comun;
using LogiMarket.Infrastructure.Pagos;
using LogiMarket.Infrastructure.Pagos.Legado;
using LogiMarket.Tests.Soporte;

namespace LogiMarket.Tests.Aplicacion;

public class PagosTests
{
    private readonly RelojControlado _reloj = new(new DateTime(2026, 11, 2, 10, 0, 0));
    private readonly ControlSimulacion _control = new();
    private readonly BitacoraTecnica _bitacora;
    private readonly SesionUsuario _sesion = new();

    public PagosTests() => _bitacora = new BitacoraTecnica(_reloj);

    private sealed class ProveedorContador : IPasarelaPago
    {
        public int Llamadas { get; private set; }
        public string Proveedor => "CONTADOR";
        public Task<ResultadoCobro> CobrarAsync(decimal monto, string clave)
        {
            Llamadas++;
            return Task.FromResult(ResultadoCobro.Aprobar(Proveedor, "CNT-1"));
        }
    }

    private sealed class ProveedorCaido : IPasarelaPago
    {
        public string Proveedor => "CAIDO";
        public Task<ResultadoCobro> CobrarAsync(decimal monto, string clave) => throw new TimeoutException("sin respuesta del banco");
    }

    private IEnumerable<IPasarelaPago> Proveedores()
    {
        yield return new PasarelaLegadaAdapter(new PasarelaLegadaSimulada(), _control, _bitacora);
        yield return new PasarelaAlternativaSimulada(_control, _bitacora);
    }

    private List<string> Pagos() => _bitacora.Entradas(CategoriasBitacora.Pago).Select(e => e.Mensaje).ToList();

    [Fact]
    public async Task P16_AmbosProveedoresCobranConElMismoContrato()
    {
        foreach (var proveedor in Proveedores())
        {
            var r = await proveedor.CobrarAsync(59.00m, "CLAVE-" + proveedor.Proveedor);
            Assert.True(r.Aprobado);
            Assert.False(string.IsNullOrEmpty(r.Referencia));
        }

        Assert.Contains(Pagos(), m => m.Contains("CobrarCentavos(5900, CLAVE-LEGADO) -> OK;LEG-"));
        Assert.Contains(Pagos(), m => m.Contains("PASARELA ALTERNATIVA"));
    }

    [Fact]
    public async Task P38_LSP_MismasPrecondicionesResultadosEIdempotencia()
    {
        foreach (var proveedor in Proveedores())
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => proveedor.CobrarAsync(0m, "X"));
            await Assert.ThrowsAsync<ArgumentException>(() => proveedor.CobrarAsync(10m, " "));

            var a = await proveedor.CobrarAsync(10m, "MISMA");
            var b = await proveedor.CobrarAsync(10m, "MISMA");
            Assert.Equal(a.Referencia, b.Referencia);

            _control.FallarCobros = true;
            var rechazo = await proveedor.CobrarAsync(10m, "OTRA-" + proveedor.Proveedor);
            Assert.False(rechazo.Aprobado);
            Assert.Null(rechazo.Referencia);
            _control.FallarCobros = false;
        }
    }

    [Fact]
    public async Task P19_DecoratorRodeaElCobroAprobadoYElRechazado()
    {
        var decorado = new AuditoriaPagoDecorator(new PasarelaLegadaAdapter(new PasarelaLegadaSimulada(), _control, _bitacora), _bitacora);

        var ok = await decorado.CobrarAsync(20m, "D-OK");
        _control.FallarCobros = true;
        var rechazo = await decorado.CobrarAsync(20m, "D-RECHAZO");

        var log = Pagos();
        Assert.StartsWith("INICIO", log[0]);
        Assert.StartsWith("PASARELA", log[1]);
        Assert.StartsWith("FIN APROBADO", log[2]);
        Assert.StartsWith("INICIO", log[3]);
        Assert.StartsWith("PASARELA", log[4]);
        Assert.StartsWith("FIN RECHAZADO TARJETA_RECHAZADA", log[5]);
        Assert.True(ok.Aprobado);
        Assert.False(rechazo.Aprobado);
        var correlaciones = _bitacora.Entradas(CategoriasBitacora.Pago).Where(e => e.Correlacion != null).Select(e => e.Correlacion).ToList();
        Assert.Equal(2, correlaciones.Distinct().Count());
    }

    [Fact]
    public async Task P19_DecoratorRegistraErrorYNoTragaLaExcepcion()
    {
        var decorado = new AuditoriaPagoDecorator(new ProveedorCaido(), _bitacora);

        await Assert.ThrowsAsync<TimeoutException>(() => decorado.CobrarAsync(15m, "D-ERR"));

        Assert.Equal(new[] { "INICIO", "ERROR" }, Pagos().Select(m => m.Split(' ')[0]));
    }

    [Theory]
    [InlineData(RolUsuario.Consulta)]
    [InlineData(RolUsuario.Bodega)]
    public async Task P22_ProxyDeniegaAntesDeLlegarAlDecoratorYAlProveedor(RolUsuario rol)
    {
        var real = new ProveedorContador();
        var cadena = new PagoAutorizadoProxy(new AuditoriaPagoDecorator(real, _bitacora), _sesion, _bitacora);
        _sesion.Iniciar(rol.ToString().ToLower(), rol);

        await Assert.ThrowsAsync<AccesoDenegadoException>(() => cadena.CobrarAsync(59m, "P22"));

        Assert.Equal(0, real.Llamadas);
        Assert.Empty(Pagos());
        Assert.Single(_bitacora.Entradas(CategoriasBitacora.Seguridad));
    }

    [Theory]
    [InlineData(RolUsuario.Ventas)]
    [InlineData(RolUsuario.Admin)]
    public async Task P22_VentasYAdminSiPuedenCobrar(RolUsuario rol)
    {
        var real = new ProveedorContador();
        var cadena = new PagoAutorizadoProxy(new AuditoriaPagoDecorator(real, _bitacora), _sesion, _bitacora);
        _sesion.Iniciar(rol.ToString().ToLower(), rol);

        var r = await cadena.CobrarAsync(59m, "P22-OK");

        Assert.True(r.Aprobado);
        Assert.Equal(1, real.Llamadas);
        Assert.Equal(new[] { "INICIO", "FIN" }, Pagos().Select(m => m.Split(' ')[0]));
    }

    [Fact]
    public void P38_CapasSinReferenciasProhibidas()
    {
        var dominio = typeof(Pedido).Assembly.GetReferencedAssemblies().Select(a => a.Name!).ToList();
        var aplicacion = typeof(IPasarelaPago).Assembly.GetReferencedAssemblies().Select(a => a.Name!).ToList();

        Assert.DoesNotContain(dominio, n => n.StartsWith("LogiMarket.") || n.Contains("EntityFramework") || n.Contains("Sqlite") || n.Contains("Windows.Forms"));
        Assert.DoesNotContain(aplicacion, n => n.Contains("EntityFramework") || n.Contains("Sqlite") || n.Contains("Windows.Forms") || n == "LogiMarket.Infrastructure");
        Assert.Contains("LogiMarket.Domain", aplicacion);
    }

    [Fact]
    public void P38_ISP_PuertosSinOperacionesQueNoAplican()
    {
        string[] prohibidos = { "Eliminar", "Actualizar", "Borrar" };
        foreach (var puerto in new[] { typeof(IPagoRepository), typeof(IAuditoriaRepository), typeof(IMovimientoInventarioRepository) })
            Assert.DoesNotContain(puerto.GetMethods(), m => prohibidos.Any(p => m.Name.StartsWith(p)));
        Assert.Single(typeof(IPasarelaPago).GetMethods(), m => !m.IsSpecialName);
    }
}
