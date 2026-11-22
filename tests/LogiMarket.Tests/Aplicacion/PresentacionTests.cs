using LogiMarket.Application.Catalogo;
using LogiMarket.Application.Comun;
using LogiMarket.Application.Consultas;
using LogiMarket.Application.Eventos;
using LogiMarket.Application.Mantenimiento;
using LogiMarket.Application.Pedidos;
using LogiMarket.Application.Pedidos.Confirmacion;
using LogiMarket.Application.Presentacion;
using LogiMarket.Domain.Enums;
using LogiMarket.Tests.Soporte;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Tests.Aplicacion;

public class PresentacionTests : IDisposable
{
    private readonly Entorno _e = new();

    public void Dispose() => _e.Dispose();

    private sealed class PantallaFalsa : IColegaPantalla
    {
        public PantallaFalsa(string nombre, params CambioNegocio[] intereses)
        {
            Nombre = nombre;
            Intereses = intereses.ToHashSet();
        }

        public string Nombre { get; }
        public IReadOnlySet<CambioNegocio> Intereses { get; }
        public List<CambioNegocio> Refrescos { get; } = new();

        public Task RefrescarAsync(CambioNegocio cambio)
        {
            Refrescos.Add(cambio);
            return Task.CompletedTask;
        }
    }

    private sealed class OutboxContador : IDisparadorOutbox
    {
        public int Llamadas { get; private set; }
        public Task<ResultadoOutbox> ProcesarAsync()
        {
            Llamadas++;
            return Task.FromResult(new ResultadoOutbox(1, 0, new List<string>()));
        }
    }

    [Fact]
    public async Task P28_AlConfirmarElMediadorRefrescaCatalogoPedidosYMetricasSinAcoplarlas()
    {
        var outbox = new OutboxContador();
        var mediador = new FormularioPrincipalMediator(outbox, _e.Reloj);
        var cobro = new PantallaFalsa("Cobro", CambioNegocio.PedidoConfirmado);
        var catalogo = new PantallaFalsa("Catálogo", CambioNegocio.PedidoConfirmado, CambioNegocio.CatalogoModificado);
        var pedidos = new PantallaFalsa("Pedidos", CambioNegocio.PedidoConfirmado);
        var metricas = new PantallaFalsa("Panel", CambioNegocio.PedidoConfirmado);
        var clientes = new PantallaFalsa("Clientes", CambioNegocio.ClientesModificados);
        foreach (var p in new[] { cobro, catalogo, pedidos, metricas, clientes }) mediador.Registrar(p);

        await mediador.NotificarAsync(cobro, CambioNegocio.PedidoConfirmado);

        Assert.Empty(cobro.Refrescos);
        Assert.Single(catalogo.Refrescos);
        Assert.Single(pedidos.Refrescos);
        Assert.Single(metricas.Refrescos);
        Assert.Empty(clientes.Refrescos);
        Assert.Equal(1, outbox.Llamadas);
        var registro = Assert.Single(mediador.Historial);
        Assert.Equal(new[] { "Catálogo", "Pedidos", "Panel" }, registro.Receptores);
    }

    [Fact]
    public async Task P28_CambiosSinEventosNoDisparanElOutbox()
    {
        var outbox = new OutboxContador();
        var mediador = new FormularioPrincipalMediator(outbox, _e.Reloj);
        var catalogo = new PantallaFalsa("Catálogo", CambioNegocio.CatalogoModificado);
        mediador.Registrar(catalogo);

        await mediador.NotificarAsync(null, CambioNegocio.CatalogoModificado);

        Assert.Equal(0, outbox.Llamadas);
        Assert.Single(catalogo.Refrescos);
    }

    [Fact]
    public async Task P28_ConElDisparadorRealLasMetricasSubenDespuesDeConfirmar()
    {
        var id = await _e.PagadoAsync("Ana", TipoEntrega.Estandar, ("KIT_TELETRABAJO", 1));
        var mediador = _e.Proveedor.GetRequiredService<FormularioPrincipalMediator>();
        var panel = new PantallaFalsa("Panel", CambioNegocio.PedidoConfirmado);
        mediador.Registrar(panel);

        await mediador.NotificarAsync(null, CambioNegocio.PedidoConfirmado);

        Assert.Single(panel.Refrescos);
        Assert.Equal(1, _e.Proveedor.GetRequiredService<LogiMarket.Application.Metricas.MetricasSesion>().PedidosPagados);
        Assert.Equal("1 procesados, 0 pendientes", mediador.Historial[0].Outbox);
        Assert.True(id > 0);
    }

    [Fact]
    public void P21_MilFilasCompartenMetadatosPorSku()
    {
        var origen = new List<OrigenFila>
        {
            new("P01", "Mouse USB", "Periféricos", "Mouse óptico con cable USB", 8, 20m),
            new("P02", "Teclado", "Periféricos", "Teclado estándar en español", 5, 30m),
            new("P03", "Memoria USB", "Almacenamiento", "Memoria flash USB", 20, 10m),
            new("P04", "Webcam", "Video", "Cámara web HD", 4, 40m),
            new("P05", "Alfombrilla", "Accesorios", "Alfombrilla para mouse", 30, 5m)
        };

        var (filas, medicion) = ComparativaFlyweight.Medir(origen, 1000);

        Assert.Equal(1000, filas.Count);
        Assert.Equal(5, medicion.ObjetosMetadatosConFlyweight);
        Assert.Equal(1000, medicion.ObjetosMetadatosSinFlyweight);
        Assert.True(medicion.MismaReferencia);
        Assert.Same(filas[0].Metadatos, filas[5].Metadatos);
        Assert.NotEqual(filas[0].Cantidad, filas[1].Cantidad);
        Assert.True(medicion.BytesConFlyweight < medicion.BytesSinFlyweight,
            $"con={medicion.BytesConFlyweight} sin={medicion.BytesSinFlyweight}");
    }

    [Fact]
    public void P21_LaFabricaDevuelveLaMismaInstanciaParaElMismoSku()
    {
        var fabrica = new CatalogoVisualFlyweightFactory();
        var a = fabrica.Obtener("P01", "Mouse USB", "Periféricos", "");
        var b = fabrica.Obtener("p01", "Mouse USB", "Periféricos", "");

        Assert.True(ReferenceEquals(a, b));
        Assert.Equal((1, 2), (fabrica.InstanciasCreadas, fabrica.Solicitudes));
    }

    [Fact]
    public async Task Catalogo_SkuDuplicadoYCicloIndirectoSeRechazan()
    {
        _e.ComoRol(RolUsuario.Admin);
        var r = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioCatalogo>()
            .CrearProductoAsync(new ProductoDto("P01", "Otro mouse", "Periféricos", "", 15m, 1, 0.2m)));
        var ciclo = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioCatalogo>()
            .AgregarAKitAsync("KIT_OFICINA", new ComponenteSolicitado("KIT_TELETRABAJO", 1)));

        Assert.False(r.Exito);
        Assert.False(ciclo.Exito);
        Assert.Contains("indirecta", ciclo.Mensaje);
    }

    [Fact]
    public async Task Catalogo_NuevoKitAnidadoYDesgloseConVisitorEIterator()
    {
        _e.ComoRol(RolUsuario.Admin);
        var creado = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioCatalogo>()
            .CrearKitAsync("KIT_ESTUDIO", "Kit Estudio", new[] { new ComponenteSolicitado("KIT_TELETRABAJO", 1), new ComponenteSolicitado("P05", 2) }));
        var desglose = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioCatalogo>().DesglosarAsync("KIT_ESTUDIO"));

        Assert.True(creado.Exito, creado.Mensaje);
        Assert.Equal(70.00m, desglose.Precio);
        Assert.Equal(1.20m, desglose.PesoKg);
        Assert.Equal(new[] { "KIT_ESTUDIO", "KIT_TELETRABAJO", "KIT_OFICINA" }, desglose.KitsVisitados);
        Assert.Equal(new[] { "P01", "P02", "P03", "P05" }, desglose.RecorridoIterador.Select(c => c.Codigo));
    }

    [Fact]
    public async Task Catalogo_ConsultaNoPuedeModificar()
    {
        _e.ComoRol(RolUsuario.Consulta);
        var r = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioCatalogo>()
            .CrearProductoAsync(new ProductoDto("P99", "Cable", "Accesorios", "", 3m, 1, 0.1m)));
        var lista = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioCatalogo>().ListarProductosAsync());

        Assert.False(r.Exito);
        Assert.Equal(5, lista.Count);
    }

    [Fact]
    public async Task Carrito_AnaCompletaConDeshacerYPlantilla()
    {
        _e.ComoRol(RolUsuario.Ventas);
        var carrito = _e.Proveedor.GetRequiredService<ServicioCarrito>();
        await carrito.SeleccionarClienteAsync(await _e.ClienteAsync("Ana"));
        await carrito.AgregarAsync("KIT_TELETRABAJO", 1);
        await carrito.AgregarAsync("P05", 3);
        Assert.True(carrito.Deshacer());
        Assert.Equal(59.00m, carrito.Estado.Total);

        var id = await carrito.GuardarAsync();
        Assert.False(carrito.TieneLineas);

        await carrito.UsarComoPlantillaAsync(id);
        carrito.ModificarCantidad("KIT_TELETRABAJO", 2);
        var copia = await carrito.GuardarAsync();

        var original = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioConsultas>().DetalleAsync(id));
        var nuevo = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioConsultas>().DetalleAsync(copia));
        Assert.Equal(1, original.Lineas[0].Cantidad);
        Assert.Equal(2, nuevo.Lineas[0].Cantidad);
        Assert.NotEqual(id, copia);
    }

    [Fact]
    public async Task Consultas_PanelYMovimientosDespuesDeUnaVenta()
    {
        var id = await _e.PagadoAsync("Carmen", TipoEntrega.Estandar, ("P03", 10));
        _e.ComoRol(RolUsuario.Consulta);

        var panel = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioConsultas>().PanelAsync());
        var movimientos = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioConsultas>().MovimientosAsync());

        Assert.Equal(1, panel.PorEstado.Single(c => c.Estado == "PAGADO").Pedidos);
        Assert.Equal(90.00m, panel.VentasCobradas);
        Assert.Equal(1, panel.EventosPendientes);
        Assert.Contains(movimientos, m => m.Sku == "P03" && m.Cantidad == -10 && m.StockResultante == 10 && m.Referencia == $"PED-{id}");
    }

    [Fact]
    public async Task Mantenimiento_RestablecerVuelveALaSemillaSoloComoAdmin()
    {
        await _e.PagadoAsync("Luis", TipoEntrega.Estandar, ("P04", 4));
        _e.ComoRol(RolUsuario.Ventas);
        var denegado = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioMantenimiento>().RestablecerAsync());

        _e.ComoRol(RolUsuario.Admin);
        var ok = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioMantenimiento>().RestablecerAsync());

        Assert.False(denegado.Exito);
        Assert.True(ok.Exito);
        Assert.Equal(4, await _e.StockAsync("P04"));
        Assert.Equal(0, await _e.PagosAsync());
    }
}
