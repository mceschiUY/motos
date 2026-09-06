using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Observaciones.Commands.Modificar
{
    public record ModificarObservacionCommand(int Id, string Texto, DateTime FechaHora, string Usuario, int EnvioId) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Observacion";
        public string? GetEntityId() => Id.ToString();
    }
}
