using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Vendedores.Commands.Eliminar
{
    public record EliminarVendedorCommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Vendedor";
        public string? GetEntityId() => Id.ToString();
    }
}
