using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Productos.Commands.Modificar
{
    public record ModificarProductoCommand(
        int Id,
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
        bool Activo,
        // Etapa C (plan §3.7) — catálogo premium. Opcionales: los payloads existentes siguen valiendo.
        bool? Destacado = null,
        bool? Novedad = null,
        string? FichaTecnica = null,
        int? ImagenPrincipalId = null) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Producto";
        public string? GetEntityId() => Id.ToString();
    }
}
