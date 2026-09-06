using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Agencias.Commands.Eliminar
{
    public record EliminarAgenciaCommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Agencia";
        public string? GetEntityId() => Id.ToString();
    }
}
