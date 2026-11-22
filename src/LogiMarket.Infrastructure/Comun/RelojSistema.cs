using LogiMarket.Application.Comun;

namespace LogiMarket.Infrastructure.Comun;

public sealed class RelojSistema : IReloj
{
    public DateTime Ahora => DateTime.Now;
}
