namespace LogiMarket.Application.Eventos;

// observer: los suscriptores no se conocen entre si; el publicador los recorre en orden de registro
public interface IObservadorEventos
{
    string Nombre { get; }
    Task ManejarAsync(EventoNegocio evento);
}

public sealed class PublicadorEventos
{
    private readonly List<IObservadorEventos> _observadores;

    public PublicadorEventos(IEnumerable<IObservadorEventos> observadores) => _observadores = observadores.ToList();

    public IReadOnlyList<string> Suscriptores => _observadores.Select(o => o.Nombre).ToList();

    public void Suscribir(IObservadorEventos observador)
    {
        if (!_observadores.Contains(observador)) _observadores.Add(observador);
    }

    public void Desuscribir(IObservadorEventos observador) => _observadores.Remove(observador);

    public async Task PublicarAsync(EventoNegocio evento)
    {
        foreach (var observador in _observadores.ToList())
            await observador.ManejarAsync(evento);
    }
}
