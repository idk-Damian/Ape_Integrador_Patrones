using System.Text.Json;
using LogiMarket.Application.Comun;
using LogiMarket.Application.Eventos;
using LogiMarket.Application.Excepciones;
using LogiMarket.Application.Interfaces;
using LogiMarket.Application.Inventario;
using LogiMarket.Application.Pagos;
using LogiMarket.Application.Pedidos.Validacion;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.Outbox;
using LogiMarket.Domain.Pagos;
using LogiMarket.Domain.Pedidos;

namespace LogiMarket.Application.Pedidos.Confirmacion;

// fachada: una sola operacion para confirmar; coordina y delega, no calcula ni escribe SQL
public sealed class ConfirmacionPedidoFacade
{
    public const string EventoPedidoPagado = TiposEvento.PedidoPagado;

    private readonly IPedidoRepository _pedidos;
    private readonly IPagoRepository _pagos;
    private readonly IEventoOutboxRepository _outbox;
    private readonly IUnitOfWork _uow;
    private readonly ServicioValidacionPedidos _validacion;
    private readonly ServicioInventario _inventario;
    private readonly IPasarelaPago _pasarela;
    private readonly IReloj _reloj;
    private readonly IBitacora _bitacora;

    public ConfirmacionPedidoFacade(IPedidoRepository pedidos, IPagoRepository pagos, IEventoOutboxRepository outbox,
        IUnitOfWork uow, ServicioValidacionPedidos validacion, ServicioInventario inventario, IPasarelaPago pasarela,
        IReloj reloj, IBitacora bitacora)
    {
        _pedidos = pedidos;
        _pagos = pagos;
        _outbox = outbox;
        _uow = uow;
        _validacion = validacion;
        _inventario = inventario;
        _pasarela = pasarela;
        _reloj = reloj;
        _bitacora = bitacora;
    }

    public async Task<ResultadoConfirmacion> ConfirmarAsync(int pedidoId, string claveIdempotencia)
    {
        if (string.IsNullOrWhiteSpace(claveIdempotencia))
            return Falla(pedidoId, "La clave de idempotencia es obligatoria.");
        var clave = claveIdempotencia.Trim();

        // 1. reintento con la misma clave: se devuelve lo que ya paso
        var previo = await _pagos.ObtenerPorClaveIdempotenciaAsync(clave);
        if (previo is not null)
            return previo.PedidoId == pedidoId
                ? Repetido(previo)
                : Falla(pedidoId, $"La clave {clave} ya se usó para el pedido #{previo.PedidoId}.");

        var pedido = await _pedidos.ObtenerPorIdAsync(pedidoId);
        if (pedido is null) return Falla(pedidoId, $"No existe el pedido #{pedidoId}.");
        if (await _pagos.ObtenerPorPedidoAsync(pedidoId) is not null)
            return Falla(pedidoId, "El pedido ya tiene un cobro registrado; no se cobra dos veces.");

        // 2. cadena de validacion: cliente, lineas, stock, permisos
        var validacion = await _validacion.ValidarAsync(pedido, Operacion.CobrarPedido);
        if (!validacion.Valido)
            return Falla(pedidoId, validacion.Motivo!, validacion);

        // 3. todo lo local va en una sola transaccion
        await using var transaccion = await _uow.IniciarTransaccionAsync();
        try
        {
            var ahora = _reloj.Ahora;
            if (pedido.EstadoNombre == "BORRADOR") pedido.Validar(ahora);

            await _inventario.DescontarPorVentaAsync(pedido, ahora);
            await _uow.GuardarCambiosAsync(); // si otro proceso vendio el mismo stock, falla aqui y no se cobra

            var cobro = await _pasarela.CobrarAsync(pedido.Total, clave);
            if (!cobro.Aprobado)
            {
                await RevertirAsync(transaccion);
                return Falla(pedidoId, $"Cobro rechazado por {cobro.Proveedor}: {cobro.Motivo}", validacion);
            }

            await _pagos.AgregarAsync(new Pago(pedido.Id, pedido.Total, cobro.Proveedor, cobro.Referencia!, clave, ahora));
            pedido.Pagar(ahora);
            await _outbox.AgregarAsync(new EventoOutbox(EventoPedidoPagado, CrearPayload(pedido, cobro), ahora));
            await _uow.GuardarCambiosAsync();
            await transaccion.ConfirmarAsync();

            return new ResultadoConfirmacion(true, pedidoId,
                $"Pedido #{pedidoId} pagado por {pedido.Total:N2} USD (ref. {cobro.Referencia}).",
                pedido.Total, cobro.Referencia, false, validacion);
        }
        catch (ConflictoDuplicadoException)
        {
            await RevertirAsync(transaccion);
            var original = await _pagos.ObtenerPorClaveIdempotenciaAsync(clave);
            return original?.PedidoId == pedidoId
                ? Repetido(original)
                : Falla(pedidoId, "Se detectó un cobro duplicado; la operación se revirtió.");
        }
        catch (ConflictoConcurrenciaException)
        {
            await RevertirAsync(transaccion);
            return Falla(pedidoId, "Otro pedido tomó el mismo stock al mismo tiempo; no se cobró. Revise existencias.");
        }
        catch (Exception ex) when (ex is DomainException or AccesoDenegadoException)
        {
            await RevertirAsync(transaccion);
            return Falla(pedidoId, ex.Message);
        }
        catch (Exception ex)
        {
            // fallo de un colaborador (base, outbox, etc.): nada queda a medias
            await RevertirAsync(transaccion);
            _bitacora.Registrar(CategoriasBitacora.Negocio, $"Confirmación del pedido #{pedidoId} revertida: {ex.Message}");
            return Falla(pedidoId, "No se pudo completar la confirmación; no se guardó ningún cambio.");
        }
    }

    private async Task RevertirAsync(ITransaccion transaccion)
    {
        await transaccion.RevertirAsync();
        _uow.DescartarCambios();
    }

    private static string CrearPayload(Pedido pedido, ResultadoCobro cobro)
        => JsonSerializer.Serialize(new
        {
            pedido.Id,
            pedido.ClienteId,
            pedido.Total,
            Referencia = cobro.Referencia,
            cobro.Proveedor
        });

    private static ResultadoConfirmacion Repetido(Pago pago)
        => new(true, pago.PedidoId,
            $"El pedido #{pago.PedidoId} ya estaba pagado con esta clave (ref. {pago.ReferenciaCobro}); no se cobró de nuevo.",
            pago.Monto, pago.ReferenciaCobro, true);

    private ResultadoConfirmacion Falla(int pedidoId, string mensaje, ResultadoValidacion? validacion = null)
    {
        _bitacora.Registrar(CategoriasBitacora.Negocio, $"Confirmación del pedido #{pedidoId} rechazada: {mensaje}");
        return new ResultadoConfirmacion(false, pedidoId, mensaje, Validacion: validacion);
    }
}
