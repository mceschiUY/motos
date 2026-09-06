using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Envios.Commands.Transicion
{
    /// <summary>
    /// Transición de ciclo de vida de Envio. La matriz (desde/hacia/campo)
    /// vive en ciclos-vida.json — una por transición, ninguna en código.
    /// </summary>
    public record TransicionEnvioCommand(int Id, string Accion) : ICommand<Result<bool>>, IAuditableRequest
    {
        public string GetEntityType() => "Envio";
        public string? GetEntityId() => Id.ToString();
    }
}
