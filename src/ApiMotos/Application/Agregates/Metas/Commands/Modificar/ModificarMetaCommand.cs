using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Metas.Commands.Modificar
{
    public record ModificarMetaCommand(int Id, int VendedorId, string Periodo, decimal ObjetivoUsd) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Meta";
        public string? GetEntityId() => Id.ToString();
    }
}
