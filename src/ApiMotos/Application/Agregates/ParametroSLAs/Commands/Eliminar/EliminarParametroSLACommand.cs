using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.ParametroSLAs.Commands.Eliminar
{
    public record EliminarParametroSLACommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "ParametroSLA";
        public string? GetEntityId() => Id.ToString();
    }
}
