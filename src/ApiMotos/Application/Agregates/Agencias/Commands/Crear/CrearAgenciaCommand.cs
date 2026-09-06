using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Agencias.Commands.Crear
{
    public record CrearAgenciaCommand(string Nombre) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Agencia";
        public string? GetEntityId() => null;
    }
}
