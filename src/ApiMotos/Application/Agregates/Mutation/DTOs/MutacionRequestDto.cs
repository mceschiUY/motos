namespace ApiMotos.Application.Agregates.Mutation.DTOs;

/// <summary>
/// Request para solicitar una nueva mutación
/// </summary>
public record MutacionRequestDto
{
    /// <summary>
    /// Solicitud en lenguaje natural
    /// Ejemplo: "Quiero que el sistema me avise cuando un cliente deba más de $5000"
    /// </summary>
    public required string Solicitud { get; init; }

    /// <summary>
    /// Contexto actual del usuario (opcional)
    /// </summary>
    public ContextoActualDto? Contexto { get; init; }
}

/// <summary>
/// Contexto actual del usuario en la aplicación
/// </summary>
public record ContextoActualDto
{
    /// <summary>
    /// Ruta actual en el frontend (ej: /clientes)
    /// </summary>
    public string? RutaActual { get; init; }

    /// <summary>
    /// Nombre del componente actual (ej: cliente-list.component.ts)
    /// </summary>
    public string? ComponenteActual { get; init; }

    /// <summary>
    /// Entidad relacionada si aplica (ej: Cliente)
    /// </summary>
    public string? EntidadRelacionada { get; init; }

    /// <summary>
    /// HTML actual del template (para comparación)
    /// </summary>
    public string? HtmlActual { get; init; }
}
