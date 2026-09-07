using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Depositos.Commands.Modificar
{
    public record ModificarDepositoCommand(int Id, string Codigo, string Nombre, string? Direccion, bool Activo) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Deposito";
        public string? GetEntityId() => Id.ToString();
    }
}
