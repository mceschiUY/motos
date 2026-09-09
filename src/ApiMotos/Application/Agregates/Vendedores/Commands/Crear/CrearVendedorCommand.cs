using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Vendedores.Commands.Crear
{
    public record CrearVendedorCommand(
        string Nombre,
        string? Telefono,
        string? Email,
        string? Zona,
        decimal ComisionPorcentaje,
        string? Usuario,
        bool Activo = true) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Vendedor";
        public string? GetEntityId() => null;
    }
}
