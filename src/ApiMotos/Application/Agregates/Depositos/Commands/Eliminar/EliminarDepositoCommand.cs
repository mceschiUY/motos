using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Depositos.Commands.Eliminar
{
    public record EliminarDepositoCommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Deposito";
        public string? GetEntityId() => Id.ToString();
    }
}
