using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Common;
using LogiMarket.Domain.Entregas;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.Pedidos.Descuentos;
using LogiMarket.Domain.Pedidos.Estados;
using LogiMarket.Domain.Pedidos.Historial;
using LogiMarket.Domain.Pedidos.Memento;

namespace LogiMarket.Domain.Pedidos;

// agregado raiz del pedido: estados, totales, historial de ediciones y clonado
public class Pedido
{
    private readonly List<LineaPedido> _lineas = new();
    private readonly List<CambioEstadoPedido> _cambiosEstado = new();
    private readonly HistorialPedido _historial = new();
    private IEstadoPedido _estado = EstadoBorrador.Instancia;

    public int Id { get; private set; }
    public int ClienteId { get; private set; }
    public CategoriaCliente CategoriaCliente { get; private set; }

    public string EstadoNombre
    {
        get => _estado.Nombre;
        private set => _estado = EstadoPedidoFactory.Crear(value);
    }

    public TipoEntrega TipoEntrega { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal Descuento { get; private set; }
    public string NombreDescuento { get; private set; } = "NORMAL";
    public decimal TarifaEnvio { get; private set; }
    public decimal Total { get; private set; }

    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaModificacion { get; private set; }
    public DateTime? FechaEntrega { get; private set; }
    public int Version { get; private set; }

    public IReadOnlyList<LineaPedido> Lineas => _lineas.AsReadOnly();
    public IReadOnlyList<CambioEstadoPedido> CambiosEstado => _cambiosEstado.AsReadOnly();

    public bool PuedeEditar => _estado.PuedeEditar;
    public bool PuedeDeshacer => PuedeEditar && _historial.PuedeDeshacer;
    public bool PuedeRehacer => PuedeEditar && _historial.PuedeRehacer;
    public int TotalUnidadesFisicas => _lineas.Sum(l => l.TotalUnidadesFisicas);

    // lo que se devuelve en una devolucion total: productos con descuento, sin flete
    public decimal ImporteProductos => Moneda.Redondear(Subtotal - Descuento);

    private Pedido() { }

    internal Pedido(int clienteId, CategoriaCliente categoria, IEnumerable<LineaPedido> lineas,
        IFabricaEntrega entrega, CalculadoraDescuento calculadora, DateTime fecha)
    {
        ClienteId = clienteId;
        CategoriaCliente = categoria;
        FechaCreacion = fecha;
        _lineas.AddRange(lineas);
        if (_lineas.Count == 0) throw new DomainException("El pedido debe tener al menos una línea.");

        Numerar();
        AplicarEntrega(entrega);
        Recalcular(calculadora);
        _cambiosEstado.Add(new CambioEstadoPedido(1, null, _estado.Nombre, fecha));
        GuardarFoto();
    }

    // ---- transiciones (State) ----

    public void Validar(DateTime fecha) => _estado.Validar(this, fecha);
    public void Despachar(DateTime fecha) => _estado.Despachar(this, fecha);
    public void Cancelar(DateTime fecha) => _estado.Cancelar(this, fecha);

    public void Pagar(DateTime fecha)
    {
        _estado.Pagar(this, fecha);
        _historial.LimpiarYBloquear();
    }

    public void Entregar(DateTime fechaEntrega)
    {
        _estado.Entregar(this, fechaEntrega);
        FechaEntrega = fechaEntrega;
    }

    // FechaModificacion registra el ultimo cambio de estado; las ediciones del borrador no la tocan
    internal void CambiarEstado(IEstadoPedido nuevo, DateTime fecha)
    {
        _cambiosEstado.Add(new CambioEstadoPedido(_cambiosEstado.Count + 1, _estado.Nombre, nuevo.Nombre, fecha));
        _estado = nuevo;
        FechaModificacion = fecha;
        Version++;
    }

    // ---- edicion del borrador ----

    public void AgregarLinea(IElementoCatalogo elemento, int cantidad, CalculadoraDescuento calculadora)
    {
        Editar(calculadora, () =>
        {
            var existente = BuscarLinea(elemento.Codigo);
            if (existente is null)
                _lineas.Add(new LineaPedido(elemento, cantidad));
            else
                existente.CambiarCantidad(existente.Cantidad + cantidad);
        });
    }

    public void ModificarCantidad(string codigo, int cantidad, CalculadoraDescuento calculadora)
    {
        Editar(calculadora, () => ObtenerLinea(codigo).CambiarCantidad(cantidad));
    }

    public void EliminarLinea(string codigo, CalculadoraDescuento calculadora)
    {
        Editar(calculadora, () =>
        {
            if (_lineas.Count == 1)
                throw new DomainException("El pedido debe conservar al menos una línea; si no lo necesita, cancélelo.");
            _lineas.Remove(ObtenerLinea(codigo));
        });
    }

    public void CambiarEntrega(IFabricaEntrega entrega, CalculadoraDescuento calculadora)
    {
        Editar(calculadora, () => AplicarEntrega(entrega));
    }

    private void Editar(CalculadoraDescuento calculadora, Action cambio)
    {
        if (!PuedeEditar)
            throw new DomainException($"No se puede editar un pedido en estado '{EstadoNombre}'.");
        if (calculadora is null) throw new DomainException("Falta la calculadora de descuentos.");

        // un pedido recien leido de la base no tiene fotos; se guarda la actual antes de cambiar
        if (_historial.Cantidad == 0) GuardarFoto();

        cambio();
        Numerar();
        Recalcular(calculadora);
        GuardarFoto();
    }

    private void Numerar()
    {
        for (int i = 0; i < _lineas.Count; i++) _lineas[i].AsignarOrden(i + 1);
    }

    private LineaPedido? BuscarLinea(string codigo)
        => _lineas.Find(l => l.ElementoCodigo == codigo?.Trim().ToUpperInvariant());

    private LineaPedido ObtenerLinea(string codigo)
        => BuscarLinea(codigo) ?? throw new DomainException($"No existe una línea con el código '{codigo}'.");

    private void AplicarEntrega(IFabricaEntrega entrega)
    {
        if (entrega is null) throw new DomainException("Debe elegir una forma de entrega.");
        var tarifa = entrega.CrearTarifa();
        if (tarifa.Familia != entrega.Tipo)
            throw new DomainException("La tarifa no pertenece a la familia de entrega elegida.");

        TipoEntrega = entrega.Tipo;
        TarifaEnvio = Moneda.Redondear(tarifa.Calcular());
    }

    private void Recalcular(CalculadoraDescuento calculadora)
    {
        Subtotal = Moneda.Redondear(_lineas.Sum(l => l.Subtotal));
        var resultado = calculadora.Calcular(new ContextoDescuento(TotalUnidadesFisicas, CategoriaCliente, Subtotal));
        NombreDescuento = resultado.Politica;
        Descuento = resultado.Monto;
        Total = Moneda.Redondear(Subtotal - Descuento + TarifaEnvio);
    }

    // requerimiento total por SKU final, usado para validar y descontar stock
    public IReadOnlyDictionary<string, int> RequerimientoPorSku()
        => _lineas
            .SelectMany(l => l.Componentes.Select(c => (c.ProductoCodigo, Cantidad: c.Cantidad * l.Cantidad)))
            .GroupBy(x => x.ProductoCodigo)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Cantidad));

    // ---- Memento ----

    private void GuardarFoto()
        => _historial.Guardar(new PedidoMemento(TipoEntrega, TarifaEnvio, _lineas.Select(l => l.Clonar()).ToList()));

    public bool Deshacer(CalculadoraDescuento calculadora) => Restaurar(_historial.Deshacer, calculadora);

    public bool Rehacer(CalculadoraDescuento calculadora) => Restaurar(_historial.Rehacer, calculadora);

    private bool Restaurar(Func<PedidoMemento?> mover, CalculadoraDescuento calculadora)
    {
        if (!PuedeEditar) throw new DomainException("Solo se puede deshacer o rehacer un borrador.");
        var foto = mover();
        if (foto is null) return false;

        TipoEntrega = foto.TipoEntrega;
        TarifaEnvio = foto.TarifaEnvio;
        _lineas.Clear();
        _lineas.AddRange(foto.Lineas.Select(l => l.Clonar()));
        Recalcular(calculadora);
        return true;
    }

    // ---- Prototype ----

    public Pedido ClonarComoBorrador(DateTime fecha)
    {
        if (EstadoNombre != EstadoBorrador.Instancia.Nombre)
            throw new DomainException("Solo se puede usar como plantilla un pedido en BORRADOR.");

        var copia = new Pedido
        {
            ClienteId = ClienteId,
            CategoriaCliente = CategoriaCliente,
            TipoEntrega = TipoEntrega,
            TarifaEnvio = TarifaEnvio,
            Subtotal = Subtotal,
            Descuento = Descuento,
            NombreDescuento = NombreDescuento,
            Total = Total,
            FechaCreacion = fecha
        };
        copia._lineas.AddRange(_lineas.Select(l => l.Clonar()));
        copia._cambiosEstado.Add(new CambioEstadoPedido(1, null, copia.EstadoNombre, fecha));
        copia.GuardarFoto();
        return copia;
    }
}
