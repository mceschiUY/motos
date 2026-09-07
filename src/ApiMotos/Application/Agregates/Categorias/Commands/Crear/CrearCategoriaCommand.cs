using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Categorias.Commands.Crear
{
    public record CrearCategoriaCommand(string Nombre, int? CategoriaPadreId, bool Activo) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Categoria";
        public string? GetEntityId() => null;
    }
}
