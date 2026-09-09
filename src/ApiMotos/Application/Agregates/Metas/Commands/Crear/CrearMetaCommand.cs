using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Metas.Commands.Crear
{
    public record CrearMetaCommand(int VendedorId, string Periodo, decimal ObjetivoUsd) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Meta";
        public string? GetEntityId() => null;
    }
}
