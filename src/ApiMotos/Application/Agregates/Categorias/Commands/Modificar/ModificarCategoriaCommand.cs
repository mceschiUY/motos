using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Categorias.Commands.Modificar
{
    public record ModificarCategoriaCommand(int Id, string Nombre, int? CategoriaPadreId, bool Activo) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Categoria";
        public string? GetEntityId() => Id.ToString();
    }
}
