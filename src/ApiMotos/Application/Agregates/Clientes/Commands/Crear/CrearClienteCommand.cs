using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Clientes.Commands.Crear
{
    public record CrearClienteCommand(string Nombre, string Telefono, string DireccionEntrega) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Cliente";
        public string? GetEntityId() => null;
    }
}
