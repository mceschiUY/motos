using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Domain.Agregates.Mutation;

namespace ApiMotos.Application.Agregates.Mutation.Commands.RevertirMutacion;

/// <summary>
/// Handler que revierte una mutación:
/// 1. Obtiene los archivos de la mutación
/// 2. Restaura cada archivo a su estado anterior
/// 3. Marca la mutación como revertida
/// </summary>
public class RevertirMutacionHandler : ICommandHandler<RevertirMutacionCommand, Result<bool>>
{
    private readonly IMutacionRepositorio _mutacionRepositorio;
    private readonly IMutationRollbackService _rollbackService;

    public RevertirMutacionHandler(
        IMutacionRepositorio mutacionRepositorio,
        IMutationRollbackService rollbackService)
    {
        _mutacionRepositorio = mutacionRepositorio;
        _rollbackService = rollbackService;
    }

    public async Task<Result<bool>> Handle(
        RevertirMutacionCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Obtener la mutación
        var mutacion = await _mutacionRepositorio.ObtenerPorIdAsync(command.MutacionId);

        if (mutacion == null)
            return Result.Fail<bool>("Mutación no encontrada");

        if (mutacion.Estado != MutacionEstado.Ejecutada)
            return Result.Fail<bool>("Solo se pueden revertir mutaciones ejecutadas");

        try
        {
            // 2. Obtener archivos para rollback
            var archivos = await _mutacionRepositorio.ObtenerArchivosPorMutacionAsync(command.MutacionId);

            if (!archivos.Any())
                return Result.Fail<bool>("No hay archivos para revertir");

            // 3. Ejecutar rollback
            var resultado = await _rollbackService.RevertirAsync(archivos, cancellationToken);

            if (resultado.IsFailed)
                return Result.Fail<bool>(resultado.Errors);

            // 4. Marcar como revertida
            mutacion.MarcarRevertida();
            await _mutacionRepositorio.ActualizarAsync(mutacion);

            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            return Result.Fail<bool>($"Error al revertir la mutación: {ex.Message}");
        }
    }
}

/// <summary>
/// Interface del servicio de rollback
/// </summary>
public interface IMutationRollbackService
{
    Task<Result<bool>> RevertirAsync(
        IEnumerable<MutacionArchivo> archivos,
        CancellationToken cancellationToken = default);
}
