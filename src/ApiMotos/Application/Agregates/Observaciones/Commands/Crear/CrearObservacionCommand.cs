using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Observaciones.Commands.Crear
{
    public record CrearObservacionCommand(string Texto, DateTime FechaHora, string Usuario, int EnvioId) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Observacion";
        public string? GetEntityId() => null;
    }
}
