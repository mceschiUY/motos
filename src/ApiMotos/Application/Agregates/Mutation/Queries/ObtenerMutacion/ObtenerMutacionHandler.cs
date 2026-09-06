using System.Text.Json;
using FluentResults;
using MediatR;
using ApiMotos.Application.Agregates.Mutation.DTOs;
using ApiMotos.Domain.Agregates.Mutation;

namespace ApiMotos.Application.Agregates.Mutation.Queries.ObtenerMutacion;

/// <summary>
/// Handler para obtener una mutación con todos sus detalles
/// </summary>
public class ObtenerMutacionHandler : IRequestHandler<ObtenerMutacionQuery, Result<MutacionResponseDto>>
{
    private readonly IMutacionRepositorio _mutacionRepositorio;

    public ObtenerMutacionHandler(IMutacionRepositorio mutacionRepositorio)
    {
        _mutacionRepositorio = mutacionRepositorio;
    }

    public async Task<Result<MutacionResponseDto>> Handle(
        ObtenerMutacionQuery request,
        CancellationToken cancellationToken)
    {
        var mutacion = await _mutacionRepositorio.ObtenerPorIdAsync(request.MutacionId);

        if (mutacion == null)
            return Result.Fail<MutacionResponseDto>("Mutación no encontrada");

        // Si tiene respuesta de Claude guardada, deserializarla
        if (!string.IsNullOrEmpty(mutacion.RespuestaClaudeJson))
        {
            try
            {
                var response = JsonSerializer.Deserialize<MutacionResponseDto>(
                    mutacion.RespuestaClaudeJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (response != null)
                    return Result.Ok(response with { MutacionId = mutacion.Id });
            }
            catch
            {
                // Si falla la deserialización, construir manualmente
            }
        }

        // Construir respuesta desde la entidad
        var dto = new MutacionResponseDto
        {
            MutacionId = mutacion.Id,
            MutationSummary = mutacion.ResumenTecnico ?? "Sin resumen disponible",
            PreviewHtml = mutacion.PreviewHtml ?? "<html><body>Preview no disponible</body></html>",
            RiskAnalysis = mutacion.RiskAnalysis,
            StructuralImpact = BuildImpactoFromEntity(mutacion),
            ArchitectureAudit = new ArchitectureAuditDto
            {
                IsCompliant = mutacion.ArchitectureCompliance >= 80,
                CompliancePercentage = mutacion.ArchitectureCompliance ?? 0,
                Warnings = new List<string>(),
                Recommendations = new List<string>(),
                Message = mutacion.ArchitectureCompliance >= 80
                    ? "Los cambios cumplen con Clean Architecture"
                    : "Revisar los cambios propuestos"
            }
        };

        return Result.Ok(dto);
    }

    private static ImpactoEstructuralDto BuildImpactoFromEntity(Mutacion mutacion)
    {
        var backendChanges = new List<CambioArchivoDto>();
        var frontendChanges = new List<CambioArchivoDto>();
        string? database = null;

        foreach (var impacto in mutacion.Impactos)
        {
            var cambio = new CambioArchivoDto
            {
                Path = impacto.RutaArchivo ?? "",
                Action = impacto.Tipo switch
                {
                    TipoImpacto.Crear => "create",
                    TipoImpacto.Modificar => "modify",
                    TipoImpacto.Eliminar => "delete",
                    _ => "modify"
                },
                Code = impacto.CodigoGenerado ?? "",
                Language = impacto.Lenguaje ?? "csharp",
                Description = impacto.Descripcion
            };

            if (impacto.Capa == CapaArquitectura.Database)
            {
                database = impacto.CodigoGenerado;
            }
            else if (impacto.Capa == CapaArquitectura.Presentation)
            {
                frontendChanges.Add(cambio);
            }
            else
            {
                backendChanges.Add(cambio);
            }
        }

        return new ImpactoEstructuralDto
        {
            Database = database,
            BackendChanges = backendChanges,
            FrontendChanges = frontendChanges
        };
    }
}
