using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Metas.Commands.Eliminar
{
    public record EliminarMetaCommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Meta";
        public string? GetEntityId() => Id.ToString();
    }
}
