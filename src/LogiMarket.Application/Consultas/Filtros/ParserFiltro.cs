using System.Globalization;

namespace LogiMarket.Application.Consultas.Filtros;

public class FiltroInvalidoException : Exception
{
    public FiltroInvalidoException(string mensaje, int posicion)
        : base($"{mensaje} (posición {posicion + 1}).") => Posicion = posicion;

    public int Posicion { get; }
}

// gramatica acotada (AND liga mas fuerte que OR; los parentesis mandan):
//   expresion   := termino ( OR termino )*
//   termino     := factor ( AND factor )*
//   factor      := '(' expresion ')' | condicion
//   condicion   := ESTADO '=' palabra | TOTAL '>=' numero | CLIENTE '=' texto
public sealed class ParserFiltro
{
    private static readonly HashSet<string> EstadosValidos = new(StringComparer.OrdinalIgnoreCase)
        { "BORRADOR", "VALIDADO", "PAGADO", "DESPACHADO", "ENTREGADO", "CANCELADO" };

    private readonly List<Token> _tokens;
    private int _actual;

    private ParserFiltro(List<Token> tokens) => _tokens = tokens;

    public static IExpresionFiltro Compilar(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) throw new FiltroInvalidoException("El filtro está vacío", 0);
        var parser = new ParserFiltro(Lexer.Analizar(texto));
        var arbol = parser.Expresion();
        if (!parser.Ver(TipoToken.Fin))
            throw new FiltroInvalidoException($"Sobra '{parser.Siguiente.Texto}' al final del filtro", parser.Siguiente.Posicion);
        return arbol;
    }

    private Token Siguiente => _tokens[_actual];
    private bool Ver(TipoToken tipo) => Siguiente.Tipo == tipo;

    private Token Consumir(TipoToken tipo, string esperado)
    {
        if (!Ver(tipo))
            throw new FiltroInvalidoException(
                Siguiente.Tipo == TipoToken.Fin ? $"El filtro terminó y se esperaba {esperado}" : $"Se esperaba {esperado} y llegó '{Siguiente.Texto}'",
                Siguiente.Posicion);
        return _tokens[_actual++];
    }

    private IExpresionFiltro Expresion()
    {
        var izquierda = Termino();
        while (Ver(TipoToken.O))
        {
            _actual++;
            izquierda = new ExpresionO(izquierda, Termino());
        }
        return izquierda;
    }

    private IExpresionFiltro Termino()
    {
        var izquierda = Factor();
        while (Ver(TipoToken.Y))
        {
            _actual++;
            izquierda = new ExpresionY(izquierda, Factor());
        }
        return izquierda;
    }

    private IExpresionFiltro Factor()
    {
        if (!Ver(TipoToken.ParentesisAbre)) return Condicion();
        _actual++;
        var interna = Expresion();
        Consumir(TipoToken.ParentesisCierra, "')'");
        return interna;
    }

    private IExpresionFiltro Condicion()
    {
        var campo = Consumir(TipoToken.Palabra, "ESTADO, TOTAL o CLIENTE");
        switch (campo.Texto.ToUpperInvariant())
        {
            case "ESTADO":
            {
                Consumir(TipoToken.Igual, "'=' después de ESTADO");
                var valor = Consumir(TipoToken.Palabra, "un estado");
                if (!EstadosValidos.Contains(valor.Texto))
                    throw new FiltroInvalidoException($"'{valor.Texto}' no es un estado válido", valor.Posicion);
                return new ExpresionEstado(valor.Texto);
            }
            case "TOTAL":
            {
                Consumir(TipoToken.MayorIgual, "'>=' después de TOTAL");
                var numero = Consumir(TipoToken.Numero, "un número");
                return new ExpresionTotalMinimo(decimal.Parse(numero.Texto, CultureInfo.InvariantCulture));
            }
            case "CLIENTE":
            {
                Consumir(TipoToken.Igual, "'=' después de CLIENTE");
                var texto = Ver(TipoToken.Cadena) ? Consumir(TipoToken.Cadena, "un texto") : Consumir(TipoToken.Palabra, "un texto");
                return new ExpresionCliente(texto.Texto);
            }
            default:
                throw new FiltroInvalidoException($"Campo desconocido '{campo.Texto}'; use ESTADO, TOTAL o CLIENTE", campo.Posicion);
        }
    }
}
