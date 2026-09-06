using ApiMotos.Domain.Agregates.Mutation;

namespace ApiMotos.Application.Agregates.Mutation.DTOs;

/// <summary>
/// Respuesta completa del análisis de mutación
/// </summary>
public record MutacionResponseDto
{
    /// <summary>
    /// ID de la mutación creada
    /// </summary>
    public int MutacionId { get; init; }

    /// <summary>
    /// Resumen técnico del cambio a realizar
    /// </summary>
    public string MutationSummary { get; init; } = string.Empty;

    /// <summary>
    /// HTML standalone para renderizar en el preview iframe (opcional, ya no se usa)
    /// </summary>
    public string PreviewHtml { get; init; } = string.Empty;

    /// <summary>
    /// Impacto estructural detallado
    /// </summary>
    public ImpactoEstructuralDto StructuralImpact { get; init; } = new();

    /// <summary>
    /// Análisis de riesgos
    /// </summary>
    public string? RiskAnalysis { get; init; }

    /// <summary>
    /// Auditoría de arquitectura (opcional, se genera con defaults)
    /// </summary>
    public ArchitectureAuditDto ArchitectureAudit { get; init; } = new() { IsCompliant = true, CompliancePercentage = 100 };
}

/// <summary>
/// Detalle del impacto estructural por capa
/// </summary>
public record ImpactoEstructuralDto
{
    /// <summary>
    /// Script SQL para migración de base de datos (si aplica)
    /// </summary>
    public string? Database { get; init; }

    /// <summary>
    /// Cambios en el backend (.NET)
    /// </summary>
    public List<CambioArchivoDto> BackendChanges { get; init; } = new();

    /// <summary>
    /// Cambios en el frontend (Angular)
    /// </summary>
    public List<CambioArchivoDto> FrontendChanges { get; init; } = new();
}

/// <summary>
/// Cambio a realizar en un archivo específico
/// </summary>
public record CambioArchivoDto
{
    /// <summary>
    /// Ruta relativa del archivo
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Acción a realizar: create, modify, delete
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// Código a escribir/insertar
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Lenguaje del código: csharp, typescript, html, scss, sql
    /// </summary>
    public string Language { get; init; } = "csharp";

    /// <summary>
    /// Descripción legible del cambio
    /// </summary>
    public string? Description { get; init; }
}

/// <summary>
/// Resultado de la auditoría de arquitectura
/// </summary>
public record ArchitectureAuditDto
{
    /// <summary>
    /// Indica si los cambios cumplen con Clean Architecture
    /// </summary>
    public bool IsCompliant { get; init; }

    /// <summary>
    /// Porcentaje de cumplimiento (0-100)
    /// </summary>
    public int CompliancePercentage { get; init; }

    /// <summary>
    /// Advertencias sobre posibles problemas
    /// </summary>
    public List<string> Warnings { get; init; } = new();

    /// <summary>
    /// Recomendaciones de mejora
    /// </summary>
    public List<string> Recommendations { get; init; } = new();

    /// <summary>
    /// Mensaje para mostrar al usuario
    /// </summary>
    public string? Message { get; init; }
}

/// <summary>
/// DTO para el historial de mutaciones
/// </summary>
public record MutacionHistorialDto
{
    public int Id { get; init; }
    public required string SolicitudOriginal { get; init; }
    public string? ResumenTecnico { get; init; }
    public MutacionEstado Estado { get; init; }
    public DateTime FechaSolicitud { get; init; }
    public DateTime? FechaEjecucion { get; init; }
    public string? UsuarioNombre { get; init; }
    public int? ArchitectureCompliance { get; init; }
    public int CantidadArchivos { get; init; }
}

/// <summary>
/// Resultado de ejecución de mutación
/// </summary>
public record EjecucionResultadoDto
{
    public bool Success { get; init; }
    public int MutacionId { get; init; }
    public int ArchivosCreados { get; init; }
    public int ArchivosModificados { get; init; }
    public int ArchivosEliminados { get; init; }
    public string? ErrorMessage { get; init; }
    public List<string> ArchivosAfectados { get; init; } = new();
}
