using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.MovimientosStock.Commands.Modificar
{
    public record ModificarMovimientoStockCommand(
        int Id,
        int VarianteId,
        int DepositoId,
        int? DepositoDestinoId,
        string Tipo,
        decimal Cantidad,
        decimal? CostoUnitario,
        string? Motivo,
        string? DocumentoOrigen,
        DateTime? Fecha,
        string? Usuario) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "MovimientoStock";
        public string? GetEntityId() => Id.ToString();
    }
}
