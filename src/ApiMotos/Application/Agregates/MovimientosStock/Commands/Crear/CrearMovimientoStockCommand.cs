using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.MovimientosStock.Commands.Crear
{
    public record CrearMovimientoStockCommand(
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
        public string? GetEntityId() => null;
    }
}
