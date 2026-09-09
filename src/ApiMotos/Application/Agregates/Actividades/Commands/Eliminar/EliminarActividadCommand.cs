using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Actividades.Commands.Eliminar
{
    public record EliminarActividadCommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Actividad";
        public string? GetEntityId() => Id.ToString();
    }
}
