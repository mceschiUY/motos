using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Colores.Commands.Eliminar
{
    public record EliminarColorCommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Color";
        public string? GetEntityId() => Id.ToString();
    }
}
