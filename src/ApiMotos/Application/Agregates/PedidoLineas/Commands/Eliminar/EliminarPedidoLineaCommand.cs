using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.PedidoLineas.Commands.Eliminar
{
    public record EliminarPedidoLineaCommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "PedidoLinea";
        public string? GetEntityId() => Id.ToString();
    }
}
