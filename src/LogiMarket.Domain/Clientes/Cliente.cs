using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Clientes;

public class Cliente
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Telefono { get; private set; } = null!;
    public CategoriaCliente Categoria { get; private set; }
    public bool Activo { get; private set; }

    private Cliente() { }

    public Cliente(string nombre, string email, string telefono, CategoriaCliente categoria = CategoriaCliente.Normal)
    {
        Editar(nombre, email, telefono, categoria);
        Activo = true;
    }

    public void Editar(string nombre, string email, string telefono, CategoriaCliente categoria)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new DomainException("El nombre del cliente es obligatorio.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("El email del cliente no es válido.");
        if (string.IsNullOrWhiteSpace(telefono)) throw new DomainException("El teléfono del cliente es obligatorio.");

        Nombre = nombre.Trim();
        Email = email.Trim().ToLowerInvariant();
        Telefono = telefono.Trim();
        Categoria = categoria;
    }

    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;
}
