using System.Text.Json;
using LogiMarket.Application.Comun;
using LogiMarket.Application.Eventos;
using LogiMarket.Application.Excepciones;
using LogiMarket.Application.Interfaces;
using LogiMarket.Application.Inventario;
using LogiMarket.Application.Pagos;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Common;
using LogiMarket.Domain.Devoluciones;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Outbox;
using LogiMarket.Domain.Pedidos;

namespace LogiMarket.Application.Devoluciones;

public sealed record SolicitudDevolucion(int PedidoId, MotivoDevolucion Motivo, bool ProductoSinUso);

public sealed record ResultadoDevolucion(bool Aceptada, string Mensaje, decimal Reembolso, int DiasDesdeEntrega,
    IReadOnlyList<string> Pasos);

// colaboradores comunes que necesita cualquier variante de devolucion
public sealed class RecursosDevolucion
{
    public RecursosDevolucion(IPedidoRepository pedidos, IDevolucionRepository devoluciones, IPagoRepository pagos,
        IEventoOutboxRepository outbox, IUnitOfWork uow, ServicioInventario inventario, IPasarelaReembolso reembolsos,
        IContextoUsuario usuario, IReloj reloj, IBitacora bitacora)
    {
        Pedidos = pedidos;
        Devoluciones = devoluciones;
        Pagos = pagos;
        Outbox = outbox;
        Uow = uow;
        Inventario = inventario;
        Reembolsos = reembolsos;
        Usuario = usuario;
        Reloj = reloj;
        Bitacora = bitacora;
    }

    public IPedidoRepository Pedidos { get; }
    public IDevolucionRepository Devoluciones { get; }
    public IPagoRepository Pagos { get; }
    public IEventoOutboxRepository Outbox { get; }
    public IUnitOfWork Uow { get; }
    public ServicioInventario Inventario { get; }
    public IPasarelaReembolso Reembolsos { get; }
    public IContextoUsuario Usuario { get; }
    public IReloj Reloj { get; }
    public IBitacora Bitacora { get; }
}

// template method: la secuencia es fija y no se sobrescribe;
// cada motivo solo decide como inspeccionar la condicion y, si hiciera falta, como calcular el reembolso
public abstract class ProcesadorDevolucionBase
{
    private readonly RecursosDevolucion _r;

    protected ProcesadorDevolucionBase(RecursosDevolucion recursos) => _r = recursos;

    public abstract MotivoDevolucion Motivo { get; }

    public async Task<ResultadoDevolucion> ProcesarAsync(SolicitudDevolucion solicitud)
    {
        var pasos = new List<string>();
        PoliticaPermisos.Exigir(_r.Usuario, Operacion.ProcesarDevolucion);

        var pedido = await _r.Pedidos.ObtenerPorIdAsync(solicitud.PedidoId)
            ?? throw new RecursoNoEncontradoException($"No existe el pedido #{solicitud.PedidoId}.");

        pasos.Add("1. Verificar elegibilidad");
        var rechazo = await VerificarElegibilidadAsync(pedido);
        if (rechazo is not null) return Rechazar(solicitud, rechazo, 0, pasos);

        int dias = (_r.Reloj.Ahora.Date - pedido.FechaEntrega!.Value.Date).Days;

        pasos.Add("2. Inspeccionar condición");
        rechazo = InspeccionarCondicion(solicitud, dias);
        if (rechazo is not null) return Rechazar(solicitud, rechazo, dias, pasos);

        pasos.Add("3. Calcular reembolso");
        var monto = Moneda.Redondear(CalcularReembolso(pedido));

        pasos.Add("4. Registrar devolución, stock y reembolso");
        await using var transaccion = await _r.Uow.IniciarTransaccionAsync();
        try
        {
            var ahora = _r.Reloj.Ahora;
            var pago = await _r.Pagos.ObtenerPorPedidoAsync(pedido.Id)
                ?? throw new RecursoNoEncontradoException($"El pedido #{pedido.Id} no tiene cobro registrado.");

            var referencia = await _r.Reembolsos.ReembolsarAsync(pago.ReferenciaCobro, monto, $"DEV-{pedido.Id}");
            await _r.Inventario.ReintegrarPorDevolucionAsync(pedido, $"DEV-{pedido.Id}", ahora);
            await _r.Devoluciones.AgregarAsync(new Devolucion(pedido.Id, Motivo, dias, solicitud.ProductoSinUso, monto, referencia, ahora));

            pasos.Add("5. Generar evento");
            await _r.Outbox.AgregarAsync(new EventoOutbox(TiposEvento.DevolucionAceptada,
                JsonSerializer.Serialize(new { pedido.Id, pedido.ClienteId, Reembolso = monto, Motivo = Motivo.ToString() }), ahora));

            await _r.Uow.GuardarCambiosAsync();
            await transaccion.ConfirmarAsync();
        }
        catch (Exception ex) when (EjecutorOperaciones.EsEsperada(ex))
        {
            await transaccion.RevertirAsync();
            _r.Uow.DescartarCambios();
            return Rechazar(solicitud, ex is ConflictoDuplicadoException
                ? "El pedido ya tiene una devolución registrada."
                : ex.Message, dias, pasos);
        }

        _r.Bitacora.Registrar(CategoriasBitacora.Negocio,
            $"Devolución {Motivo} del pedido #{pedido.Id} aceptada: reembolso {monto:N2} USD a los {dias} días.");
        return new ResultadoDevolucion(true,
            $"Devolución aceptada. Reembolso de {monto:N2} USD (sin tarifa de envío).", monto, dias, pasos);
    }

    // paso fijo: igual para todos los motivos
    private async Task<string?> VerificarElegibilidadAsync(Pedido pedido)
    {
        if (pedido.EstadoNombre != "ENTREGADO" || pedido.FechaEntrega is null)
            return $"Solo se devuelven pedidos ENTREGADOS; el pedido está {pedido.EstadoNombre}.";
        if (await _r.Devoluciones.ObtenerPorPedidoAsync(pedido.Id) is not null)
            return "El pedido ya fue devuelto; la devolución es única.";
        return null;
    }

    // paso variable: cada motivo define su plazo y sus condiciones
    protected abstract string? InspeccionarCondicion(SolicitudDevolucion solicitud, int diasDesdeEntrega);

    // por defecto se devuelve lo pagado por productos (con descuento) sin la tarifa de envio
    protected virtual decimal CalcularReembolso(Pedido pedido) => pedido.ImporteProductos;

    private ResultadoDevolucion Rechazar(SolicitudDevolucion solicitud, string motivo, int dias, List<string> pasos)
    {
        _r.Bitacora.Registrar(CategoriasBitacora.Negocio,
            $"Devolución {solicitud.Motivo} del pedido #{solicitud.PedidoId} rechazada: {motivo}");
        return new ResultadoDevolucion(false, motivo, 0m, dias, pasos);
    }
}

public sealed class DevolucionPorDefecto : ProcesadorDevolucionBase
{
    public const int PlazoDias = 30;

    public DevolucionPorDefecto(RecursosDevolucion recursos) : base(recursos) { }

    public override MotivoDevolucion Motivo => MotivoDevolucion.Defecto;

    protected override string? InspeccionarCondicion(SolicitudDevolucion solicitud, int dias)
        => dias <= PlazoDias ? null : $"Por defecto se acepta hasta {PlazoDias} días; han pasado {dias}.";
}

public sealed class DevolucionPorArrepentimiento : ProcesadorDevolucionBase
{
    public const int PlazoDias = 7;

    public DevolucionPorArrepentimiento(RecursosDevolucion recursos) : base(recursos) { }

    public override MotivoDevolucion Motivo => MotivoDevolucion.Arrepentimiento;

    protected override string? InspeccionarCondicion(SolicitudDevolucion solicitud, int dias)
    {
        if (dias > PlazoDias) return $"Por arrepentimiento se acepta hasta {PlazoDias} días; han pasado {dias}.";
        return solicitud.ProductoSinUso ? null : "Por arrepentimiento el producto debe estar sin uso.";
    }
}
