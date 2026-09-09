using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.PedidoLineas.Commands.Modificar
{
    public record ModificarPedidoLineaCommand(
        int Id,
        int PedidoId,
        int VarianteId,
        decimal Cantidad,
        decimal? PrecioUnitarioUsd) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "PedidoLinea";
        public string? GetEntityId() => Id.ToString();
    }
}
