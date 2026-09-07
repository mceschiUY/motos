using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Variantes.Commands.Eliminar
{
    public record EliminarVarianteCommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Variante";
        public string? GetEntityId() => Id.ToString();
    }
}
