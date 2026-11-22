using LogiMarket.Application.Eventos;

namespace LogiMarket.Application.Presentacion;

public enum CambioNegocio
{
    SesionIniciada,
    CatalogoModificado,
    ClientesModificados,
    CarritoModificado,
    PedidoGuardado,
    PedidoValidado,
    PedidoConfirmado,
    PedidoCancelado,
    PedidoDespachado,
    PedidoEntregado,
    DevolucionProcesada,
    EventosProcesados,
    DatosRestablecidos
}

// cada pantalla es un colega: avisa al mediador y se refresca cuando el mediador se lo pide
public interface IColegaPantalla
{
    string Nombre { get; }
    IReadOnlySet<CambioNegocio> Intereses { get; }
    Task RefrescarAsync(CambioNegocio cambio);
}

public interface IMediadorPantallas
{
    void Registrar(IColegaPantalla colega);
    void Desregistrar(IColegaPantalla colega);
    Task NotificarAsync(IColegaPantalla? emisor, CambioNegocio cambio);
}

// permite al mediador vaciar el outbox despues de una operacion confirmada
public interface IDisparadorOutbox
{
    Task<ResultadoOutbox> ProcesarAsync();
}

public sealed record RegistroMediacion(DateTime Fecha, string Emisor, CambioNegocio Cambio, IReadOnlyList<string> Receptores, string? Outbox);
