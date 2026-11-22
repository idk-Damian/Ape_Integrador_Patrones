namespace LogiMarket.Application.Consultas.Filtros;

// interpreter: cada nodo del arbol sabe evaluarse contra un pedido
public interface IExpresionFiltro
{
    bool Evaluar(PedidoResumen pedido);
    string Describir();
}

public sealed class ExpresionEstado : IExpresionFiltro
{
    private readonly string _estado;
    public ExpresionEstado(string estado) => _estado = estado.ToUpperInvariant();
    public bool Evaluar(PedidoResumen p) => string.Equals(p.Estado, _estado, StringComparison.OrdinalIgnoreCase);
    public string Describir() => $"ESTADO = {_estado}";
}

public sealed class ExpresionTotalMinimo : IExpresionFiltro
{
    private readonly decimal _minimo;
    public ExpresionTotalMinimo(decimal minimo) => _minimo = minimo;
    public bool Evaluar(PedidoResumen p) => p.Total >= _minimo;
    public string Describir() => $"TOTAL >= {_minimo:0.##}";
}

// coincide con el nombre completo o con cualquiera de sus palabras, sin importar mayusculas
public sealed class ExpresionCliente : IExpresionFiltro
{
    private readonly string _texto;
    public ExpresionCliente(string texto) => _texto = texto.Trim();

    public bool Evaluar(PedidoResumen p)
        => string.Equals(p.Cliente, _texto, StringComparison.OrdinalIgnoreCase)
           || p.Cliente.Split(' ', StringSplitOptions.RemoveEmptyEntries)
               .Any(parte => string.Equals(parte, _texto, StringComparison.OrdinalIgnoreCase));

    public string Describir() => $"CLIENTE = \"{_texto}\"";
}

public sealed class ExpresionY : IExpresionFiltro
{
    private readonly IExpresionFiltro _izq, _der;
    public ExpresionY(IExpresionFiltro izq, IExpresionFiltro der) { _izq = izq; _der = der; }
    public bool Evaluar(PedidoResumen p) => _izq.Evaluar(p) && _der.Evaluar(p);
    public string Describir() => $"({_izq.Describir()} AND {_der.Describir()})";
}

public sealed class ExpresionO : IExpresionFiltro
{
    private readonly IExpresionFiltro _izq, _der;
    public ExpresionO(IExpresionFiltro izq, IExpresionFiltro der) { _izq = izq; _der = der; }
    public bool Evaluar(PedidoResumen p) => _izq.Evaluar(p) || _der.Evaluar(p);
    public string Describir() => $"({_izq.Describir()} OR {_der.Describir()})";
}
