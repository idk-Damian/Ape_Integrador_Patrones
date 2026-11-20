using LogiMarket.Domain.Clientes;
using LogiMarket.Domain.Common;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.Pedidos.Descuentos;
using LogiMarket.Domain.Pedidos.Estados;
using LogiMarket.Domain.Pedidos.Memento;

namespace LogiMarket.Domain.Pedidos;

/// <summary>
/// Agregado raíz de Pedido. Integra State, Prototype, Memento, Strategy.
/// </summary>
public class Pedido
{
    private readonly List<LineaPedido> _lineas = new();
    private IEstadoPedido _estado = EstadoBorrador.Instancia;
    private readonly HistorialPedido _historial = new();

    public int Id { get; private set; }
    public int ClienteId { get; private set; }
    public CategoriaCliente CategoriaCliente { get; private set; }

    // --- Estado (State pattern) ---
    public string EstadoNombre
    {
        get => _estado.Nombre;
        private set => _estado = EstadoPedidoFactory.Crear(value);
    }

    public bool PuedeEditar => _estado.PuedeEditar;

    // --- Líneas ---
    public IReadOnlyList<LineaPedido> Lineas => _lineas.AsReadOnly();

    // --- Totales ---
    public decimal Subtotal { get; private set; }
    public decimal Descuento { get; private set; }
    public string NombreDescuento { get; private set; } = "NORMAL";
    public decimal TarifaEnvio { get; private set; }
    public decimal Total { get; private set; }

    // --- Entrega ---
    public string TipoEntrega { get; private set; } = Enums.TipoEntrega.RecogerEnTienda.ToString();

    // --- Fechas ---
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaModificacion { get; private set; }

    // --- EF Core ---
    private Pedido() { }

    internal Pedido(int clienteId, CategoriaCliente categoriaCliente,
        List<LineaPedido> lineas, TipoEntrega tipoEntrega,
        IEstrategiaDescuento estrategia)
    {
        if (lineas.Count == 0) throw new DomainException("El pedido debe tener al menos una línea.");

        ClienteId = clienteId;
        CategoriaCliente = categoriaCliente;
        TipoEntrega = tipoEntrega.ToString();
        FechaCreacion = DateTime.UtcNow;
        _lineas.AddRange(lineas);

        RecalcularTotales(estrategia);
        GuardarSnapshot();
    }

    // ===== TRANSICIONES DE ESTADO (State) =====
    public void Validar() => _estado.Validar(this);
    public void Pagar() => _estado.Pagar(this);
    public void Despachar() => _estado.Despachar(this);
    public void Entregar() => _estado.Entregar(this);
    public void Cancelar() => _estado.Cancelar(this);

    internal void CambiarEstado(IEstadoPedido nuevoEstado)
    {
        _estado = nuevoEstado;
        FechaModificacion = DateTime.UtcNow;
    }

    // ===== EDICIÓN (solo en BORRADOR) =====
    public void AgregarLinea(LineaPedido linea, IEstrategiaDescuento? estrategia = null)
    {
        if (!PuedeEditar)
            throw new DomainException($"No se puede editar un pedido en estado '{EstadoNombre}'.");

        _lineas.Add(linea);
        RecalcularTotales(estrategia);
        GuardarSnapshot();
    }

    public void EliminarLinea(string elementoCodigo, IEstrategiaDescuento? estrategia = null)
    {
        if (!PuedeEditar)
            throw new DomainException($"No se puede editar un pedido en estado '{EstadoNombre}'.");

        var linea = _lineas.Find(l => l.ElementoCodigo == elementoCodigo)
            ?? throw new DomainException($"No se encontró la línea con código '{elementoCodigo}'.");

        _lineas.Remove(linea);
        RecalcularTotales(estrategia);
        GuardarSnapshot();
    }

    // ===== CÁLCULOS =====
    public int TotalUnidadesFisicas
    {
        get
        {
            int total = 0;
            foreach (var l in _lineas) total += l.TotalUnidadesFisicas;
            return total;
        }
    }

    private void RecalcularTotales(IEstrategiaDescuento? estrategia = null)
    {
        Subtotal = 0m;
        foreach (var l in _lineas)
            Subtotal += l.Subtotal;
        Subtotal = Moneda.Redondear(Subtotal);

        estrategia ??= SelectorDescuento.CrearEstandar()
            .Seleccionar(TotalUnidadesFisicas, CategoriaCliente);

        NombreDescuento = estrategia.Nombre;
        Descuento = estrategia.CalcularDescuento(Subtotal);
        Total = Moneda.Redondear(Subtotal - Descuento + TarifaEnvio);
    }

    public void EstablecerTarifaEnvio(decimal tarifa)
    {
        if (tarifa < 0) throw new DomainException("La tarifa de envío no puede ser negativa.");
        TarifaEnvio = Moneda.Redondear(tarifa);
        Total = Moneda.Redondear(Subtotal - Descuento + TarifaEnvio);
    }

    // ===== PROTOTYPE =====
    /// <summary>
    /// Patrón PROTOTYPE – Clon profundo en BORRADOR. No copia ID ni estado de pago.
    /// Modificar la copia no altera la plantilla.
    /// </summary>
    public Pedido ClonarComoBorrador()
    {
        if (EstadoNombre != "BORRADOR")
            throw new DomainException("Solo se puede clonar un pedido en estado BORRADOR.");

        var clon = new Pedido
        {
            ClienteId = ClienteId,
            CategoriaCliente = CategoriaCliente,
            TipoEntrega = TipoEntrega,
            Subtotal = Subtotal,
            Descuento = Descuento,
            NombreDescuento = NombreDescuento,
            TarifaEnvio = TarifaEnvio,
            Total = Total,
            FechaCreacion = DateTime.UtcNow,
            _estado = EstadoBorrador.Instancia
        };

        foreach (var linea in _lineas)
            clon._lineas.Add(linea.ClonarProfundo());

        return clon;
    }

    // ===== MEMENTO =====
    private void GuardarSnapshot()
    {
        var lineasSnap = _lineas.Select(l =>
            new PedidoMemento.LineaSnapshot(
                l.ElementoCodigo, l.ElementoNombre, l.PrecioUnitario,
                l.Cantidad, l.EsKit,
                l.Componentes.Select(c =>
                    new PedidoMemento.ComponenteSnap(
                        c.ProductoCodigo, c.ProductoNombre,
                        c.PrecioUnitario, c.Cantidad, c.PesoKg))
                    .ToList().AsReadOnly()))
            .ToList().AsReadOnly();

        var memento = new PedidoMemento(
            ClienteId, TipoEntrega,
            Subtotal, Descuento, NombreDescuento,
            TarifaEnvio, Total, lineasSnap);

        _historial.Guardar(memento);
    }

    public bool Deshacer()
    {
        if (!PuedeEditar) throw new DomainException("No se puede deshacer en este estado.");
        var memento = _historial.Deshacer();
        if (memento is null) return false;
        RestaurarDesdeMemento(memento);
        return true;
    }

    public bool Rehacer()
    {
        if (!PuedeEditar) throw new DomainException("No se puede rehacer en este estado.");
        var memento = _historial.Rehacer();
        if (memento is null) return false;
        RestaurarDesdeMemento(memento);
        return true;
    }

    private void RestaurarDesdeMemento(PedidoMemento m)
    {
        TipoEntrega = m.TipoEntrega;
        Subtotal = m.Subtotal;
        Descuento = m.Descuento;
        NombreDescuento = m.NombreDescuento;
        TarifaEnvio = m.TarifaEnvio;
        Total = m.Total;
        FechaModificacion = DateTime.UtcNow;

        _lineas.Clear();
        foreach (var ls in m.Lineas)
        {
            var componentes = ls.Componentes.Select(c =>
                new LineaComponenteSnapshot(c.Codigo, c.Nombre, c.PrecioUnitario, c.Cantidad, c.PesoKg));
            var linea = new LineaPedido(
                ls.ElementoCodigo, ls.ElementoNombre, ls.PrecioUnitario,
                ls.Cantidad, ls.EsKit, componentes);
            _lineas.Add(linea);
        }
    }

    internal void BloquearHistorial() => _historial.Bloquear();

    public bool PuedeDeshacer => _historial.PuedeDeshacer;
    public bool PuedeRehacer => _historial.PuedeRehacer;

    // ===== Carga desde persistencia =====
    internal void CargarLineas(IEnumerable<LineaPedido> lineas)
    {
        _lineas.Clear();
        _lineas.AddRange(lineas);
    }
}
