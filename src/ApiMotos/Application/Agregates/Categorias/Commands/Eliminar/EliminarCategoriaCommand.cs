using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Categorias.Commands.Eliminar
{
    public record EliminarCategoriaCommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Categoria";
        public string? GetEntityId() => Id.ToString();
    }
}
