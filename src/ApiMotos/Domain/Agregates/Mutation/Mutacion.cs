using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Mutation;

/// <summary>
/// Entidad principal que representa una solicitud de mutación del sistema.
/// Una mutación es un cambio solicitado en lenguaje natural que ZAS analiza,
/// previsualiza y ejecuta sobre el código fuente.
/// </summary>
public class Mutacion : BaseEntity<int>
{
    /// <summary>
    /// Solicitud original del usuario en lenguaje natural
    /// Ejemplo: "Quiero que el sistema me avise cuando un cliente deba más de $5000"
    /// </summary>
    public string SolicitudOriginal { get; private set; } = string.Empty;

    /// <summary>
    /// Resumen técnico generado por Claude
    /// Describe los cambios a realizar en términos técnicos
    /// </summary>
    public string? ResumenTecnico { get; private set; }

    /// <summary>
    /// Estado actual de la mutación en su ciclo de vida
    /// </summary>
    public MutacionEstado Estado { get; private set; }

    /// <summary>
    /// Fecha y hora de la solicitud
    /// </summary>
    public DateTime FechaSolicitud { get; private set; }

    /// <summary>
    /// Fecha y hora del análisis (cuando Claude respondió)
    /// </summary>
    public DateTime? FechaAnalisis { get; private set; }

    /// <summary>
    /// Fecha y hora de ejecución (cuando se escribieron los archivos)
    /// </summary>
    public DateTime? FechaEjecucion { get; private set; }

    /// <summary>
    /// ID del usuario que solicitó la mutación
    /// </summary>
    public int UsuarioId { get; private set; }

    /// <summary>
    /// Nombre del usuario para referencia rápida
    /// </summary>
    public string? UsuarioNombre { get; private set; }

    /// <summary>
    /// HTML del preview generado para el iframe
    /// </summary>
    public string? PreviewHtml { get; private set; }

    /// <summary>
    /// Porcentaje de cumplimiento con Clean Architecture (0-100)
    /// </summary>
    public int? ArchitectureCompliance { get; private set; }

    /// <summary>
    /// Análisis de riesgos generado
    /// </summary>
    public string? RiskAnalysis { get; private set; }

    /// <summary>
    /// Mensaje de error si la mutación falló
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Contexto del proyecto (ruta, componente actual, etc.) en JSON
    /// </summary>
    public string? ContextoJson { get; private set; }

    /// <summary>
    /// Respuesta completa de Claude en JSON (para debugging/historial)
    /// </summary>
    public string? RespuestaClaudeJson { get; private set; }

    // Navegación
    private readonly List<MutacionImpacto> _impactos = new();
    public IReadOnlyCollection<MutacionImpacto> Impactos => _impactos.AsReadOnly();

    private readonly List<MutacionArchivo> _archivos = new();
    public IReadOnlyCollection<MutacionArchivo> Archivos => _archivos.AsReadOnly();

    // Constructor privado para EF
    private Mutacion() { }

    /// <summary>
    /// Crea una nueva mutación a partir de la solicitud del usuario
    /// </summary>
    public static Mutacion Crear(string solicitud, int usuarioId, string? usuarioNombre, string? contextoJson = null)
    {
        if (string.IsNullOrWhiteSpace(solicitud))
            throw new ArgumentException("La solicitud no puede estar vacía", nameof(solicitud));

        return new Mutacion
        {
            SolicitudOriginal = solicitud.Trim(),
            Estado = MutacionEstado.Pendiente,
            FechaSolicitud = DateTime.UtcNow,
            UsuarioId = usuarioId,
            UsuarioNombre = usuarioNombre,
            ContextoJson = contextoJson
        };
    }

    /// <summary>
    /// Registra el resultado del análisis de Claude
    /// </summary>
    public void RegistrarAnalisis(
        string resumenTecnico,
        string previewHtml,
        int architectureCompliance,
        string? riskAnalysis,
        string respuestaClaudeJson)
    {
        ResumenTecnico = resumenTecnico;
        PreviewHtml = previewHtml;
        ArchitectureCompliance = architectureCompliance;
        RiskAnalysis = riskAnalysis;
        RespuestaClaudeJson = respuestaClaudeJson;
        FechaAnalisis = DateTime.UtcNow;
        Estado = MutacionEstado.Analizada;
    }

    /// <summary>
    /// Marca la mutación como previsualizada (usuario vio el preview)
    /// </summary>
    public void MarcarPrevisualizada()
    {
        if (Estado != MutacionEstado.Analizada)
            throw new InvalidOperationException("Solo se puede previsualizar una mutación analizada");

        Estado = MutacionEstado.Previsualizada;
    }

    /// <summary>
    /// Marca la mutación como ejecutada exitosamente
    /// </summary>
    public void MarcarEjecutada()
    {
        if (Estado != MutacionEstado.Previsualizada && Estado != MutacionEstado.Analizada)
            throw new InvalidOperationException("Solo se puede ejecutar una mutación analizada o previsualizada");

        Estado = MutacionEstado.Ejecutada;
        FechaEjecucion = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca la mutación como fallida
    /// </summary>
    public void MarcarFallida(string errorMessage)
    {
        Estado = MutacionEstado.Fallida;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Marca la mutación como revertida
    /// </summary>
    public void MarcarRevertida()
    {
        if (Estado != MutacionEstado.Ejecutada)
            throw new InvalidOperationException("Solo se puede revertir una mutación ejecutada");

        Estado = MutacionEstado.Revertida;
    }

    /// <summary>
    /// Agrega un impacto estructural a la mutación
    /// </summary>
    public void AgregarImpacto(MutacionImpacto impacto)
    {
        _impactos.Add(impacto);
    }

    /// <summary>
    /// Agrega un archivo generado/modificado a la mutación
    /// </summary>
    public void AgregarArchivo(MutacionArchivo archivo)
    {
        _archivos.Add(archivo);
    }
}
