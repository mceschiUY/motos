using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Mutation.Commands.RevertirMutacion;

/// <summary>
/// Command para revertir una mutación ejecutada (rollback).
/// Restaura los archivos a su estado anterior.
/// </summary>
public record RevertirMutacionCommand(
    int MutacionId
) : ICommand<Result<bool>>, IAuditableRequest
{
    public string GetEntityType() => "Mutacion";
    public string? GetEntityId() => MutacionId.ToString();
}
