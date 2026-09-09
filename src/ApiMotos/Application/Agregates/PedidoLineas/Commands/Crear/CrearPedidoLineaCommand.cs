using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.PedidoLineas.Commands.Crear
{
    /// <summary>PrecioUnitarioUsd opcional: si no viene, PedidoLineaHooks copia el precio
    /// de lista de la variante (es lo que hace el armado de pedido).</summary>
    public record CrearPedidoLineaCommand(
        int PedidoId,
        int VarianteId,
        decimal Cantidad,
        decimal? PrecioUnitarioUsd) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "PedidoLinea";
        public string? GetEntityId() => null;
    }
}
