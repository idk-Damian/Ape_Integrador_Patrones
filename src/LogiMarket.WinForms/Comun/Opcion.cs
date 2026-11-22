namespace LogiMarket.WinForms.Comun;

// elemento de combo con un valor y el texto que ve el usuario
internal sealed record Opcion<T>(T Valor, string Texto)
{
    public override string ToString() => Texto;
}
