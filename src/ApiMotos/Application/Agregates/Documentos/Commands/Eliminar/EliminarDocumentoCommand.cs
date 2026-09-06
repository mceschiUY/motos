using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Documentos.Commands.Eliminar
{
    public record EliminarDocumentoCommand(int Id) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Documento";
        public string? GetEntityId() => Id.ToString();
    }
}
