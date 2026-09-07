using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Depositos.Commands.Crear
{
    public record CrearDepositoCommand(string Codigo, string Nombre, string? Direccion, bool Activo) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Deposito";
        public string? GetEntityId() => null;
    }
}
