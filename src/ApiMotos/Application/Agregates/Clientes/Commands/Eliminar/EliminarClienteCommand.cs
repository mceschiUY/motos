using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Clientes.Commands.Eliminar
{
    public record EliminarClienteCommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Cliente";
        public string? GetEntityId() => Id.ToString();
    }
}
