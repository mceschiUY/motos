using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Variantes.Commands.Modificar
{
    public record ModificarVarianteCommand(
        int Id,
        int ProductoId,
        int? TallaId,
        int? ColorId,
        string Sku,
        string? CodigoBarras,
        decimal CostoEstandar,
        decimal PrecioLista,
        bool Activo) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Variante";
        public string? GetEntityId() => Id.ToString();
    }
}
