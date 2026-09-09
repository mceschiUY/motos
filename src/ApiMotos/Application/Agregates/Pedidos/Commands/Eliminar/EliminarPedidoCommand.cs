using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Pedidos.Commands.Eliminar
{
    public record EliminarPedidoCommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Pedido";
        public string? GetEntityId() => Id.ToString();
    }
}
