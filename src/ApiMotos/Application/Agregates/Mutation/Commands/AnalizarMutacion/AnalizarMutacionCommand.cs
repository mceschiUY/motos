using FluentResults;
using ApiMotos.Application.Agregates.Mutation.DTOs;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Mutation.Commands.AnalizarMutacion;

/// <summary>
/// Command para analizar una solicitud de mutación con Claude AI.
/// Genera el preview y el impacto estructural sin ejecutar cambios.
/// </summary>
public record AnalizarMutacionCommand(
    string Solicitud,
    ContextoActualDto? Contexto
) : ICommand<Result<MutacionResponseDto>>, IAuditableRequest
{
    public string GetEntityType() => "Mutacion";
    public string? GetEntityId() => null;
}
