using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Vendedores.Commands.Modificar
{
    public record ModificarVendedorCommand(
        int Id,
        string Nombre,
        string? Telefono,
        string? Email,
        string? Zona,
        decimal ComisionPorcentaje,
        string? Usuario,
        bool Activo = true) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Vendedor";
        public string? GetEntityId() => Id.ToString();
    }
}
