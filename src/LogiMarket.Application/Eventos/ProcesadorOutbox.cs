using LogiMarket.Application.Comun;
using LogiMarket.Application.Interfaces;

namespace LogiMarket.Application.Eventos;

public sealed record ResultadoOutbox(int Procesados, int Fallidos, IReadOnlyList<string> Detalle);

// lee los eventos pendientes que dejo la transaccion de negocio y los publica a los observadores;
// si un observador falla, el evento queda pendiente con el error y se reintenta despues
public sealed class ProcesadorOutbox
{
    private static readonly SemaphoreSlim UnoALaVez = new(1, 1);

    private readonly IEventoOutboxRepository _outbox;
    private readonly PublicadorEventos _publicador;
    private readonly IUnitOfWork _uow;
    private readonly IReloj _reloj;
    private readonly IBitacora _bitacora;

    public ProcesadorOutbox(IEventoOutboxRepository outbox, PublicadorEventos publicador, IUnitOfWork uow,
        IReloj reloj, IBitacora bitacora)
    {
        _outbox = outbox;
        _publicador = publicador;
        _uow = uow;
        _reloj = reloj;
        _bitacora = bitacora;
    }

    public async Task<ResultadoOutbox> ProcesarPendientesAsync()
    {
        await UnoALaVez.WaitAsync();
        try
        {
            int ok = 0, fallidos = 0;
            var detalle = new List<string>();

            foreach (var pendiente in await _outbox.ObtenerPendientesAsync())
            {
                var evento = new EventoNegocio(pendiente.IdEvento, pendiente.TipoEvento, pendiente.Payload, pendiente.FechaCreacion);
                try
                {
                    await _publicador.PublicarAsync(evento);
                    pendiente.MarcarProcesado(_reloj.Ahora);
                    await _uow.GuardarCambiosAsync();
                    ok++;
                    detalle.Add($"{pendiente.TipoEvento} #{pendiente.Id}: procesado");
                    _bitacora.Registrar(CategoriasBitacora.Outbox, $"Evento {pendiente.TipoEvento} #{pendiente.Id} procesado.", evento.Id.ToString("N"));
                }
                catch (Exception ex)
                {
                    // lo que algun observador alcanzo a preparar se descarta; solo se anota el fallo
                    _uow.DescartarCambios();
                    var recargado = await _outbox.ObtenerPorIdAsync(pendiente.Id);
                    recargado?.RegistrarFallo(ex.Message);
                    await _uow.GuardarCambiosAsync();
                    fallidos++;
                    detalle.Add($"{pendiente.TipoEvento} #{pendiente.Id}: pendiente ({ex.Message})");
                    _bitacora.Registrar(CategoriasBitacora.Outbox, $"Evento {pendiente.TipoEvento} #{pendiente.Id} falló: {ex.Message}", evento.Id.ToString("N"));
                }
            }
            return new ResultadoOutbox(ok, fallidos, detalle);
        }
        finally
        {
            UnoALaVez.Release();
        }
    }
}
