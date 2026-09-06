using System.Text.Json;
using FluentResults;
using ApiMotos.Application.Agregates.Mutation.DTOs;
using ApiMotos.Application.Common;
using ApiMotos.Domain.Agregates.Mutation;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Mutation.Commands.AnalizarMutacion;

/// <summary>
/// Handler que procesa la solicitud de mutación:
/// 1. Crea la entidad Mutacion
/// 2. Llama al servicio de análisis de Claude
/// 3. Registra el resultado y devuelve el preview
/// </summary>
public class AnalizarMutacionHandler : ICommandHandler<AnalizarMutacionCommand, Result<MutacionResponseDto>>
{
    private readonly IMutacionRepositorio _mutacionRepositorio;
    private readonly IMutationAnalyzerService _analyzerService;
    private readonly ICurrentUserService _currentUserService;

    public AnalizarMutacionHandler(
        IMutacionRepositorio mutacionRepositorio,
        IMutationAnalyzerService analyzerService,
        ICurrentUserService currentUserService)
    {
        _mutacionRepositorio = mutacionRepositorio;
        _analyzerService = analyzerService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MutacionResponseDto>> Handle(
        AnalizarMutacionCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Crear la entidad de mutación
        var contextoJson = command.Contexto != null
            ? JsonSerializer.Serialize(command.Contexto)
            : null;

        var mutacion = Mutacion.Crear(
            command.Solicitud,
            _currentUserService.UserId ?? 0,
            _currentUserService.UserName,
            contextoJson
        );

        var mutacionId = await _mutacionRepositorio.CrearAsync(mutacion);

        try
        {
            // 2. Analizar con Claude
            var analisisResult = await _analyzerService.AnalizarSolicitudAsync(
                command.Solicitud,
                command.Contexto,
                cancellationToken
            );

            if (analisisResult.IsFailed)
            {
                mutacion.MarcarFallida(analisisResult.Errors.First().Message);
                await _mutacionRepositorio.ActualizarAsync(mutacion);
                return Result.Fail<MutacionResponseDto>(analisisResult.Errors);
            }

            var response = analisisResult.Value;

            // 3. Registrar el análisis en la entidad
            mutacion.RegistrarAnalisis(
                response.MutationSummary,
                response.PreviewHtml,
                response.ArchitectureAudit.CompliancePercentage,
                response.RiskAnalysis,
                JsonSerializer.Serialize(response)
            );

            // 4. Guardar impactos estructurales
            var orden = 0;
            if (response.StructuralImpact.Database != null)
            {
                var impactoDb = MutacionImpacto.Crear(
                    mutacionId,
                    CapaArquitectura.Database,
                    TipoImpacto.Modificar,
                    "Script de migración de base de datos",
                    null,
                    response.StructuralImpact.Database,
                    "sql",
                    orden++
                );
                await _mutacionRepositorio.AgregarImpactoAsync(impactoDb);
            }

            foreach (var change in response.StructuralImpact.BackendChanges)
            {
                var capa = DeterminarCapa(change.Path);
                var tipo = change.Action switch
                {
                    "create" => TipoImpacto.Crear,
                    "modify" => TipoImpacto.Modificar,
                    "delete" => TipoImpacto.Eliminar,
                    _ => TipoImpacto.Modificar
                };

                var impacto = MutacionImpacto.Crear(
                    mutacionId,
                    capa,
                    tipo,
                    change.Description ?? $"{change.Action} {change.Path}",
                    change.Path,
                    change.Code,
                    change.Language,
                    orden++
                );
                await _mutacionRepositorio.AgregarImpactoAsync(impacto);
            }

            foreach (var change in response.StructuralImpact.FrontendChanges)
            {
                var tipo = change.Action switch
                {
                    "create" => TipoImpacto.Crear,
                    "modify" => TipoImpacto.Modificar,
                    "delete" => TipoImpacto.Eliminar,
                    _ => TipoImpacto.Modificar
                };

                var impacto = MutacionImpacto.Crear(
                    mutacionId,
                    CapaArquitectura.Presentation,
                    tipo,
                    change.Description ?? $"{change.Action} {change.Path}",
                    change.Path,
                    change.Code,
                    change.Language,
                    orden++
                );
                await _mutacionRepositorio.AgregarImpactoAsync(impacto);
            }

            await _mutacionRepositorio.ActualizarAsync(mutacion);

            // 5. Devolver respuesta con el ID
            return Result.Ok(response with { MutacionId = mutacionId });
        }
        catch (Exception ex)
        {
            mutacion.MarcarFallida(ex.Message);
            await _mutacionRepositorio.ActualizarAsync(mutacion);
            return Result.Fail<MutacionResponseDto>($"Error al analizar la mutación: {ex.Message}");
        }
    }

    private static CapaArquitectura DeterminarCapa(string path)
    {
        var pathLower = path.ToLowerInvariant();

        if (pathLower.Contains("/domain/"))
            return CapaArquitectura.Domain;
        if (pathLower.Contains("/application/"))
            return CapaArquitectura.Application;
        if (pathLower.Contains("/infrastructure/"))
            return CapaArquitectura.Infrastructure;
        if (pathLower.Contains("/controllers/"))
            return CapaArquitectura.Presentation;

        return CapaArquitectura.Application;
    }
}

/// <summary>
/// Interface del servicio de análisis con Claude
/// </summary>
public interface IMutationAnalyzerService
{
    Task<Result<MutacionResponseDto>> AnalizarSolicitudAsync(
        string solicitud,
        ContextoActualDto? contexto,
        CancellationToken cancellationToken = default);
}
