using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Observaciones.Commands.Eliminar
{
    public record EliminarObservacionCommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Observacion";
        public string? GetEntityId() => Id.ToString();
    }
}
