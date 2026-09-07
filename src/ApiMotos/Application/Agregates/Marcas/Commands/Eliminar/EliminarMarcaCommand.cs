using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Marcas.Commands.Eliminar
{
    public record EliminarMarcaCommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Marca";
        public string? GetEntityId() => Id.ToString();
    }
}
