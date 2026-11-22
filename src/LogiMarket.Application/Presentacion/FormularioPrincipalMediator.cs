using LogiMarket.Application.Comun;
using LogiMarket.Application.Eventos;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Application.Presentacion;

// mediator: ninguna pantalla llama a otra; todas avisan aqui y este decide a quien refrescar
public sealed class FormularioPrincipalMediator : IMediadorPantallas
{
    private static readonly HashSet<CambioNegocio> GeneranEventos = new()
    {
        CambioNegocio.PedidoConfirmado, CambioNegocio.PedidoDespachado, CambioNegocio.DevolucionProcesada
    };

    private readonly List<IColegaPantalla> _colegas = new();
    private readonly List<RegistroMediacion> _historial = new();
    private readonly IDisparadorOutbox _outbox;
    private readonly IReloj _reloj;

    public FormularioPrincipalMediator(IDisparadorOutbox outbox, IReloj reloj)
    {
        _outbox = outbox;
        _reloj = reloj;
    }

    public IReadOnlyList<RegistroMediacion> Historial => _historial;
    public IReadOnlyList<string> Colegas => _colegas.Select(c => c.Nombre).ToList();

    public void Registrar(IColegaPantalla colega)
    {
        if (!_colegas.Contains(colega)) _colegas.Add(colega);
    }

    public void Desregistrar(IColegaPantalla colega) => _colegas.Remove(colega);

    public async Task NotificarAsync(IColegaPantalla? emisor, CambioNegocio cambio)
    {
        string? resumenOutbox = null;
        if (GeneranEventos.Contains(cambio))
        {
            // etapa de postconfirmacion: primero se publican los eventos, luego se refrescan las vistas
            var r = await _outbox.ProcesarAsync();
            resumenOutbox = $"{r.Procesados} procesados, {r.Fallidos} pendientes";
        }

        var receptores = _colegas.Where(c => !ReferenceEquals(c, emisor) && c.Intereses.Contains(cambio)).ToList();
        foreach (var colega in receptores)
            await colega.RefrescarAsync(cambio);

        _historial.Add(new RegistroMediacion(_reloj.Ahora, emisor?.Nombre ?? "sistema", cambio,
            receptores.Select(r => r.Nombre).ToList(), resumenOutbox));
    }
}

public sealed class DisparadorOutbox : IDisparadorOutbox
{
    private readonly IServiceScopeFactory _alcances;

    public DisparadorOutbox(IServiceScopeFactory alcances) => _alcances = alcances;

    public async Task<ResultadoOutbox> ProcesarAsync()
    {
        using var alcance = _alcances.CreateScope();
        return await alcance.ServiceProvider.GetRequiredService<ProcesadorOutbox>().ProcesarPendientesAsync();
    }
}
