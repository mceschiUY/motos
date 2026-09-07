using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Productos.Commands.Crear
{
    public record CrearProductoCommand(
        string Codigo,
        string Nombre,
        int MarcaId,
        int CategoriaId,
        string? Descripcion,
        string Genero,
        string? Temporada,
        string? Material,
        int? PesoGramos,
        string? TipoCasco,
        string? Homologacion,
        bool? HomologacionVigente,
        DateTime? FechaVencHomologacion,
        bool Activo) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Producto";
        public string? GetEntityId() => null;
    }
}
