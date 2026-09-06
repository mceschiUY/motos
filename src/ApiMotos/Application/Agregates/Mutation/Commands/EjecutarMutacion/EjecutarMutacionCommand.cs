using FluentResults;
using ApiMotos.Application.Agregates.Mutation.DTOs;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Mutation.Commands.EjecutarMutacion;

/// <summary>
/// Command para ejecutar una mutación previamente analizada.
/// Escribe físicamente los archivos generados.
/// </summary>
public record EjecutarMutacionCommand(
    int MutacionId
) : ICommand<Result<EjecucionResultadoDto>>, IAuditableRequest
{
    public string GetEntityType() => "Mutacion";
    public string? GetEntityId() => MutacionId.ToString();
}
