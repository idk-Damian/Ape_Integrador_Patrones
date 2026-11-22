using LogiMarket.Application.Comun;
using LogiMarket.Application.Excepciones;
using LogiMarket.Application.Interfaces;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Clientes;
using LogiMarket.Domain.Enums;

namespace LogiMarket.Application.Clientes;

public sealed record ClienteDto(int Id, string Nombre, string Email, string Telefono, CategoriaCliente Categoria, bool Activo);

public sealed class ServicioClientes
{
    private readonly IClienteRepository _clientes;
    private readonly IUnitOfWork _uow;
    private readonly IContextoUsuario _usuario;
    private readonly EjecutorOperaciones _ejecutor;

    public ServicioClientes(IClienteRepository clientes, IUnitOfWork uow, IContextoUsuario usuario, EjecutorOperaciones ejecutor)
    {
        _clientes = clientes;
        _uow = uow;
        _usuario = usuario;
        _ejecutor = ejecutor;
    }

    public async Task<IReadOnlyList<ClienteDto>> ListarAsync(string? texto = null)
    {
        PoliticaPermisos.Exigir(_usuario, Operacion.ConsultarInformacion);
        return (await _clientes.ListarAsync(texto)).Select(Mapear).ToList();
    }

    public Task<ResultadoOperacion> GuardarAsync(ClienteDto dto)
        => _ejecutor.EjecutarAsync("Guardar cliente", async () =>
        {
            PoliticaPermisos.Exigir(_usuario, Operacion.GestionarClientes);
            if (await _clientes.ExisteEmailAsync(dto.Email, dto.Id == 0 ? null : dto.Id))
                throw new ConflictoDuplicadoException($"Ya existe un cliente con el email {dto.Email}.");

            if (dto.Id == 0)
            {
                await _clientes.AgregarAsync(new Cliente(dto.Nombre, dto.Email, dto.Telefono, dto.Categoria));
                await _uow.GuardarCambiosAsync();
                return $"Cliente {dto.Nombre} registrado.";
            }

            var cliente = await _clientes.ObtenerPorIdAsync(dto.Id) ?? throw new RecursoNoEncontradoException($"No existe el cliente {dto.Id}.");
            cliente.Editar(dto.Nombre, dto.Email, dto.Telefono, dto.Categoria);
            if (dto.Activo) cliente.Activar(); else cliente.Desactivar();
            await _uow.GuardarCambiosAsync();
            return $"Cliente {cliente.Nombre} actualizado.";
        });

    private static ClienteDto Mapear(Cliente c) => new(c.Id, c.Nombre, c.Email, c.Telefono, c.Categoria, c.Activo);
}
