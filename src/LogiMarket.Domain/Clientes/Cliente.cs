using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Clientes;

public class Cliente
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public CategoriaCliente Categoria { get; private set; }

    private Cliente() { }

    public Cliente(string nombre, string email, CategoriaCliente categoria = CategoriaCliente.Normal)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new DomainException("El nombre del cliente es obligatorio.");
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("El email del cliente es obligatorio.");

        Nombre = nombre;
        Email = email;
        Categoria = categoria;
    }

    public void CambiarCategoria(CategoriaCliente nuevaCategoria) => Categoria = nuevaCategoria;
}
