using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Agencias.Commands.Modificar
{
    public record ModificarAgenciaCommand(int Id, string Nombre) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Agencia";
        public string? GetEntityId() => Id.ToString();
    }
}
