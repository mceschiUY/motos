using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Productos.Commands.Eliminar
{
    public record EliminarProductoCommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Producto";
        public string? GetEntityId() => Id.ToString();
    }
}
