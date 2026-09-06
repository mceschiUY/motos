using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Envios.Commands.Eliminar
{
    public record EliminarEnvioCommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Envio";
        public string? GetEntityId() => Id.ToString();
    }
}
