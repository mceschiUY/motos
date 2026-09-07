using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Tallas.Commands.Eliminar
{
    public record EliminarTallaCommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Talla";
        public string? GetEntityId() => Id.ToString();
    }
}
