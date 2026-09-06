using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Documentos.Commands.Cargar
{
    public record CargarDocumentoCommand(
        string Nombre,
        string Extension,
        byte[] Contenido,
        string MimeType,
        int RelacionId,
        string RelacionNombre
    ) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Documento";
        public string? GetEntityId() => null;
    }
}
