using LogiMarket.Application.Comun;
using LogiMarket.Application.Eventos;
using LogiMarket.Application.Metricas;
using LogiMarket.Application.Notificaciones;
using LogiMarket.Domain.Clientes;
using LogiMarket.Domain.Enums;
using LogiMarket.Infrastructure.Comun;
using LogiMarket.Infrastructure.Notificaciones;
using LogiMarket.Tests.Soporte;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Tests.Aplicacion;

public class EventosTests : IDisposable
{
    private readonly Entorno _e = new();

    public void Dispose() => _e.Dispose();

    private List<string> Notificaciones() => _e.Bitacora.Entradas(CategoriasBitacora.Notificacion).Select(n => n.Mensaje).ToList();

    [Fact]
    public async Task P30_PedidoPagadoAvisaUnaVezALosTresSuscriptores()
    {
        await _e.PagadoAsync("Ana", TipoEntrega.Estandar, ("KIT_TELETRABAJO", 1));
        var metricas = _e.Proveedor.GetRequiredService<MetricasSesion>();

        var primero = await _e.ProcesarOutboxAsync();
        var segundo = await _e.ProcesarOutboxAsync();

        Assert.Equal((1, 0), (primero.Procesados, primero.Fallidos));
        Assert.Equal(0, segundo.Procesados);
        Assert.Single(Notificaciones());
        Assert.Equal(1, await _e.ConContextoAsync(ctx => ctx.Auditorias.CountAsync(a => a.Accion == TiposEvento.PedidoPagado)));
        Assert.Equal(1, metricas.PedidosPagados);
        Assert.Equal(0, await _e.ConContextoAsync(ctx => ctx.EventosOutbox.CountAsync(e => e.FechaProcesado == null)));

        var suscriptores = await _e.EnAlcanceAsync(sp => Task.FromResult(sp.GetRequiredService<PublicadorEventos>().Suscriptores));
        Assert.Equal(3, suscriptores.Count);
    }

    [Fact]
    public async Task P30_TransaccionFallidaNoDejaEventoDeExito()
    {
        var id = await _e.BorradorAsync("Luis", TipoEntrega.Estandar, ("P04", 1));
        _e.Control.FallarCobros = true;
        await _e.ComoAsync(RolUsuario.Ventas, async () =>
            await _e.EnAlcanceAsync(sp => sp.GetRequiredService<LogiMarket.Application.Pedidos.Confirmacion.ConfirmacionPedidoFacade>().ConfirmarAsync(id, "X")));

        var r = await _e.ProcesarOutboxAsync();

        Assert.Equal(0, r.Procesados);
        Assert.Empty(Notificaciones());
    }

    [Fact]
    public async Task P37_FallaElNotificadorYAlReintentarSeProcesaUnaSolaVez()
    {
        await _e.PagadoAsync("Ana", TipoEntrega.Estandar, ("KIT_TELETRABAJO", 1));
        _e.Control.FallarNotificaciones = true;

        var fallido = await _e.ProcesarOutboxAsync();
        var pendiente = await _e.ConContextoAsync(ctx => ctx.EventosOutbox.SingleAsync());

        Assert.Equal((0, 1), (fallido.Procesados, fallido.Fallidos));
        Assert.Null(pendiente.FechaProcesado);
        Assert.Equal(1, pendiente.Intentos);
        Assert.Contains("SMTP", pendiente.UltimoError);
        Assert.Equal(0, await _e.ConContextoAsync(ctx => ctx.Auditorias.CountAsync()));

        _e.Control.FallarNotificaciones = false;
        var recuperado = await _e.ProcesarOutboxAsync();
        var otraVez = await _e.ProcesarOutboxAsync();
        var final = await _e.ConContextoAsync(ctx => ctx.EventosOutbox.SingleAsync());

        Assert.Equal(1, recuperado.Procesados);
        Assert.Equal(0, otraVez.Procesados);
        Assert.NotNull(final.FechaProcesado);
        Assert.Equal(2, final.Intentos);
        Assert.Single(Notificaciones());
        Assert.Equal(1, await _e.ConContextoAsync(ctx => ctx.Auditorias.CountAsync()));
        Assert.Equal(1, await _e.PagosAsync());
    }

    [Fact]
    public async Task P11_ElCanalSeEligeFueraDelFormularioYCadaCreadorEntregaSuNotificador()
    {
        var configuracion = _e.Proveedor.GetRequiredService<ConfiguracionNotificaciones>();
        var id = await _e.PagadoAsync("Ana", TipoEntrega.Estandar, ("P05", 1));
        await _e.ProcesarOutboxAsync();

        configuracion.Canal = CanalesNotificacion.Sms;
        await _e.ComoAsync(RolUsuario.Bodega, async () =>
            await _e.EnAlcanceAsync(sp => sp.GetRequiredService<LogiMarket.Application.Logistica.ServicioLogistica>().DespacharAsync(id)));
        await _e.ProcesarOutboxAsync();

        var avisos = Notificaciones();
        Assert.StartsWith("EMAIL a ana@demo.local", avisos[0]);
        Assert.StartsWith("SMS a 0990000001", avisos[1]);
        Assert.Contains("STD-", avisos[1]);
    }

    [Fact]
    public void P11_CadaCreadorFabricaElNotificadorDeSuCanal()
    {
        var bitacora = new BitacoraTecnica(_e.Reloj);
        var control = new ControlSimulacion();
        CreadorNotificador[] creadores = { new CreadorNotificadorEmail(bitacora, control), new CreadorNotificadorSms(bitacora, control) };

        Assert.IsType<NotificadorEmailSimulado>(creadores[0].CrearNotificador());
        Assert.IsType<NotificadorSmsSimulado>(creadores[1].CrearNotificador());
        Assert.All(creadores, c => Assert.Equal(c.Canal, c.CrearNotificador().Canal));
    }

    [Fact]
    public async Task P15_MetricasSonLaMismaInstanciaYSoportanHilosConcurrentes()
    {
        MetricasSesion desdeCatalogo, desdePedidos;
        using (var a = _e.Proveedor.CreateScope()) desdeCatalogo = a.ServiceProvider.GetRequiredService<MetricasSesion>();
        using (var b = _e.Proveedor.CreateScope()) desdePedidos = b.ServiceProvider.GetRequiredService<MetricasSesion>();

        Assert.Same(desdeCatalogo, desdePedidos);

        await Task.WhenAll(Enumerable.Range(0, 1000).Select(_ => Task.Run(() =>
        {
            desdeCatalogo.RegistrarEvento(Guid.NewGuid(), TiposEvento.PedidoPagado);
            desdePedidos.RegistrarNotificacion();
        })));
        var repetido = Guid.NewGuid();
        desdeCatalogo.RegistrarEvento(repetido, TiposEvento.DespachoRegistrado);
        desdePedidos.RegistrarEvento(repetido, TiposEvento.DespachoRegistrado);

        Assert.Equal(1000, desdePedidos.PedidosPagados);
        Assert.Equal(1000, desdeCatalogo.Notificaciones);
        Assert.Equal(1, desdeCatalogo.Despachos);
        Assert.Equal(1001, desdeCatalogo.EventosProcesados);
    }
}
