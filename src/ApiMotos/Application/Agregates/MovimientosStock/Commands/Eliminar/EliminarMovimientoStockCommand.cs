using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.MovimientosStock.Commands.Eliminar
{
    public record EliminarMovimientoStockCommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "MovimientoStock";
        public string? GetEntityId() => Id.ToString();
    }
}
