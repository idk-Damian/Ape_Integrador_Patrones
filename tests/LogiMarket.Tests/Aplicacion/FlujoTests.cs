using LogiMarket.Application.Interfaces;
using LogiMarket.Application.Logistica;
using LogiMarket.Application.Pedidos;
using LogiMarket.Application.Pedidos.Confirmacion;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Enums;
using LogiMarket.Tests.Soporte;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Tests.Aplicacion;

public class FlujoTests : IDisposable
{
    private readonly Entorno _e = new();

    public void Dispose() => _e.Dispose();

    private Task<LogiMarket.Application.Comun.ResultadoOperacion> LogisticaAsync(Func<ServicioLogistica, Task<LogiMarket.Application.Comun.ResultadoOperacion>> accion)
        => _e.EnAlcanceAsync(sp => accion(sp.GetRequiredService<ServicioLogistica>()));

    [Fact]
    public async Task P09_BodegaDespachaYEntregaSoloEnLaSecuenciaPermitida()
    {
        var id = await _e.BorradorAsync("Luis", TipoEntrega.Expres, ("P04", 1));

        _e.ComoRol(RolUsuario.Bodega);
        var sinPago = await LogisticaAsync(l => l.DespacharAsync(id));
        Assert.False(sinPago.Exito);
        Assert.Equal("BORRADOR", await _e.EstadoAsync(id));

        _e.ComoRol(RolUsuario.Ventas);
        await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ConfirmacionPedidoFacade>().ConfirmarAsync(id, "LUIS-P09"));
        var ventasDespacha = await LogisticaAsync(l => l.DespacharAsync(id));
        Assert.False(ventasDespacha.Exito);

        _e.ComoRol(RolUsuario.Bodega);
        var despacho = await LogisticaAsync(l => l.DespacharAsync(id));
        Assert.True(despacho.Exito, despacho.Mensaje);
        Assert.Contains("ENVIO EXPRES", despacho.Mensaje);

        _e.Reloj.Avanzar(TimeSpan.FromDays(1));
        var entrega = await LogisticaAsync(l => l.EntregarAsync(id));
        Assert.True(entrega.Exito, entrega.Mensaje);
        Assert.Equal("ENTREGADO", await _e.EstadoAsync(id));

        var guia = await _e.ConContextoAsync(ctx => ctx.Despachos.SingleAsync(d => d.PedidoId == id));
        Assert.StartsWith("EXP-", guia.NumeroGuia);
        Assert.Equal(new DateTime(2026, 11, 3, 10, 0, 0), guia.FechaEstimada);
        Assert.NotNull(guia.FechaEntrega);
    }

    [Fact]
    public async Task P23_LaCadenaSeDetieneEnElPrimerErrorYAvanzaAlCorregirlo()
    {
        var id = await _e.BorradorAsync("Ana", TipoEntrega.Estandar, ("KIT_TELETRABAJO", 6));
        await _e.EnAlcanceAsync(async sp =>
        {
            var ana = await sp.GetRequiredService<IClienteRepository>().ObtenerPorIdAsync(await _e.ClienteAsync("Ana"));
            ana!.Desactivar();
            await sp.GetRequiredService<IUnitOfWork>().GuardarCambiosAsync();
            return 0;
        });
        _e.ComoRol(RolUsuario.Ventas);

        var primero = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioPedidos>().ValidarAsync(id));
        Assert.False(primero.Valido);
        Assert.Equal("Cliente activo", primero.EslabonFallido);
        Assert.Equal(new[] { "Cliente activo" }, primero.Recorrido);

        await _e.EnAlcanceAsync(async sp =>
        {
            var ana = await sp.GetRequiredService<IClienteRepository>().ObtenerPorIdAsync(await _e.ClienteAsync("Ana"));
            ana!.Activar();
            await sp.GetRequiredService<IUnitOfWork>().GuardarCambiosAsync();
            return 0;
        });

        var segundo = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioPedidos>().ValidarAsync(id));
        Assert.Equal("Stock disponible", segundo.EslabonFallido);
        Assert.Equal(new[] { "Cliente activo", "Líneas y cantidades", "Stock disponible" }, segundo.Recorrido);
        Assert.Empty(_e.MensajesPago());
        Assert.Equal("BORRADOR", await _e.EstadoAsync(id));
    }

    [Fact]
    public async Task ValidarPedidoCorrectoRecorreLosCuatroEslabones()
    {
        var id = await _e.BorradorAsync("Luis", TipoEntrega.Estandar, ("P05", 3));
        _e.ComoRol(RolUsuario.Ventas);

        var r = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioPedidos>().ValidarAsync(id));

        Assert.True(r.Valido);
        Assert.Equal(4, r.Recorrido.Count);
        Assert.Equal("VALIDADO", await _e.EstadoAsync(id));
    }

    [Fact]
    public async Task ConsultaNoPuedeCrearNiCobrar()
    {
        var id = await _e.BorradorAsync("Luis", TipoEntrega.Estandar, ("P05", 1));
        _e.ComoRol(RolUsuario.Consulta);

        var cobro = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ConfirmacionPedidoFacade>().ConfirmarAsync(id, "CONSULTA-1"));

        Assert.False(cobro.Exito);
        Assert.Equal("Autorización y estado", cobro.Validacion!.EslabonFallido);
        Assert.Empty(_e.MensajesPago());
        await Assert.ThrowsAsync<LogiMarket.Application.Excepciones.AccesoDenegadoException>(() =>
            _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioPedidos>()
                .CrearBorradorAsync(1, new[] { new LineaSolicitada("P05", 1) }, TipoEntrega.Estandar)));
    }

    [Fact]
    public async Task InicioDeSesionConHashYMensajeGenerico()
    {
        var ok = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioAutenticacion>().IniciarSesionAsync("bodega", "Bodega123!"));
        Assert.True(ok.Exito);
        Assert.Equal(RolUsuario.Bodega, _e.Sesion.Rol);

        _e.Sesion.Cerrar();
        var mala = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioAutenticacion>().IniciarSesionAsync("bodega", "otra"));
        var inexistente = await _e.EnAlcanceAsync(sp => sp.GetRequiredService<ServicioAutenticacion>().IniciarSesionAsync("nadie", "x"));
        Assert.Equal(mala.Mensaje, inexistente.Mensaje);
        Assert.False(_e.Sesion.Autenticado);
    }
}
