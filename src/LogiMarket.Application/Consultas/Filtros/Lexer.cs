using System.Text;

namespace LogiMarket.Application.Consultas.Filtros;

public enum TipoToken { Palabra, Numero, Cadena, Igual, MayorIgual, ParentesisAbre, ParentesisCierra, Y, O, Fin }

public sealed record Token(TipoToken Tipo, string Texto, int Posicion);

// convierte el texto en tokens; cualquier simbolo fuera de la gramatica se rechaza aqui
public static class Lexer
{
    public static List<Token> Analizar(string texto)
    {
        var tokens = new List<Token>();
        int i = 0;
        while (i < texto.Length)
        {
            char c = texto[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }

            int inicio = i;
            if (c == '(') { tokens.Add(new(TipoToken.ParentesisAbre, "(", i++)); continue; }
            if (c == ')') { tokens.Add(new(TipoToken.ParentesisCierra, ")", i++)); continue; }
            if (c == '=') { tokens.Add(new(TipoToken.Igual, "=", i++)); continue; }
            if (c == '>')
            {
                if (i + 1 < texto.Length && texto[i + 1] == '=') { tokens.Add(new(TipoToken.MayorIgual, ">=", i)); i += 2; continue; }
                throw new FiltroInvalidoException("Solo se admite el comparador '>='", i);
            }
            if (c == '"')
            {
                var sb = new StringBuilder();
                i++;
                while (i < texto.Length && texto[i] != '"') sb.Append(texto[i++]);
                if (i >= texto.Length) throw new FiltroInvalidoException("Falta cerrar las comillas", inicio);
                i++;
                tokens.Add(new(TipoToken.Cadena, sb.ToString(), inicio));
                continue;
            }
            if (char.IsDigit(c))
            {
                while (i < texto.Length && (char.IsDigit(texto[i]) || texto[i] == '.')) i++;
                var numero = texto[inicio..i];
                if (numero.Count(ch => ch == '.') > 1) throw new FiltroInvalidoException($"Número mal escrito '{numero}'", inicio);
                tokens.Add(new(TipoToken.Numero, numero, inicio));
                continue;
            }
            if (char.IsLetter(c))
            {
                while (i < texto.Length && (char.IsLetterOrDigit(texto[i]) || texto[i] == '_')) i++;
                var palabra = texto[inicio..i];
                var tipo = palabra.ToUpperInvariant() switch
                {
                    "AND" => TipoToken.Y,
                    "OR" => TipoToken.O,
                    _ => TipoToken.Palabra
                };
                tokens.Add(new(tipo, palabra, inicio));
                continue;
            }
            throw new FiltroInvalidoException($"Carácter no permitido '{c}'", i);
        }
        tokens.Add(new(TipoToken.Fin, "", texto.Length));
        return tokens;
    }
}
