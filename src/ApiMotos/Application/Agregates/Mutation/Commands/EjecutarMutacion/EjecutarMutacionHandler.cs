using FluentResults;
using ApiMotos.Application.Agregates.Mutation.DTOs;
using ApiMotos.Application.Common;
using ApiMotos.Domain.Agregates.Mutation;

namespace ApiMotos.Application.Agregates.Mutation.Commands.EjecutarMutacion;

/// <summary>
/// Handler que ejecuta una mutación:
/// 1. Obtiene la mutación con sus impactos
/// 2. Escribe los archivos físicamente
/// 3. Registra los archivos para rollback
/// </summary>
public class EjecutarMutacionHandler : ICommandHandler<EjecutarMutacionCommand, Result<EjecucionResultadoDto>>
{
    private readonly IMutacionRepositorio _mutacionRepositorio;
    private readonly IMutationExecutorService _executorService;

    public EjecutarMutacionHandler(
        IMutacionRepositorio mutacionRepositorio,
        IMutationExecutorService executorService)
    {
        _mutacionRepositorio = mutacionRepositorio;
        _executorService = executorService;
    }

    public async Task<Result<EjecucionResultadoDto>> Handle(
        EjecutarMutacionCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Obtener la mutación
        var mutacion = await _mutacionRepositorio.ObtenerPorIdAsync(command.MutacionId);

        if (mutacion == null)
            return Result.Fail<EjecucionResultadoDto>("Mutación no encontrada");

        if (mutacion.Estado != MutacionEstado.Analizada && mutacion.Estado != MutacionEstado.Previsualizada)
            return Result.Fail<EjecucionResultadoDto>(
                $"La mutación no puede ser ejecutada. Estado actual: {mutacion.Estado}");

        try
        {
            // 2. Ejecutar los cambios
            var resultado = await _executorService.EjecutarMutacionAsync(mutacion, cancellationToken);

            if (resultado.IsFailed)
            {
                mutacion.MarcarFallida(resultado.Errors.First().Message);
                await _mutacionRepositorio.ActualizarAsync(mutacion);
                return Result.Fail<EjecucionResultadoDto>(resultado.Errors);
            }

            // 3. Registrar archivos para rollback
            foreach (var archivo in resultado.Value.ArchivosAfectados)
            {
                // El servicio ya registró los archivos, solo actualizamos la entidad
            }

            // 4. Marcar como ejecutada
            mutacion.MarcarEjecutada();
            await _mutacionRepositorio.ActualizarAsync(mutacion);

            return Result.Ok(resultado.Value);
        }
        catch (Exception ex)
        {
            mutacion.MarcarFallida(ex.Message);
            await _mutacionRepositorio.ActualizarAsync(mutacion);
            return Result.Fail<EjecucionResultadoDto>($"Error al ejecutar la mutación: {ex.Message}");
        }
    }
}

/// <summary>
/// Interface del servicio de ejecución de mutaciones
/// </summary>
public interface IMutationExecutorService
{
    Task<Result<EjecucionResultadoDto>> EjecutarMutacionAsync(
        Mutacion mutacion,
        CancellationToken cancellationToken = default);
}
