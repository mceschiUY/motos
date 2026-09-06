// ═══════════════════════════════════════════════════════════════════════════════
// CONTEXT BUFFER - Embudo de Creacion de Universos Kosmos
// Almacena y gestiona multiples fuentes para la creacion de un universo coherente
// ═══════════════════════════════════════════════════════════════════════════════

namespace ApiMotos.Domain.Kosmos.UniverseCreation;

/// <summary>
/// Buffer de contexto que almacena todas las fuentes para crear un universo.
/// Implementa el patron "Embudo de Creacion" con 3 fases:
/// 1. Ingesta: Recoleccion de fuentes
/// 2. Validacion: Deteccion de conflictos
/// 3. Fusion: Merge semantico con Claude Code
/// </summary>
public class ContextBuffer
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UniverseName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Estado del buffer en el embudo de creacion
    /// </summary>
    public BufferStatus Status { get; set; } = BufferStatus.Ingesta;

    /// <summary>
    /// Coleccion de todas las fuentes agregadas
    /// </summary>
    public List<SourceEntry> Sources { get; set; } = new();

    /// <summary>
    /// Conflictos detectados entre fuentes
    /// </summary>
    public List<ConflictReport> Conflicts { get; set; } = new();

    /// <summary>
    /// Puntos de tension que requieren decision del usuario
    /// </summary>
    public List<TensionPoint> TensionPoints { get; set; } = new();

    /// <summary>
    /// Resultado del merge (universe.json generado)
    /// </summary>
    public string? MergedUniverseJson { get; set; }

    /// <summary>
    /// Resumen de las fuentes procesadas
    /// </summary>
    public SourcesSummary GetSummary() => new()
    {
        TotalSources = Sources.Count,
        ByType = Sources.GroupBy(s => s.Type)
                       .ToDictionary(g => g.Key.ToString(), g => g.Count()),
        TotalConflicts = Conflicts.Count,
        UnresolvedTensions = TensionPoints.Count(t => !t.IsResolved),
        Status = Status
    };
}

/// <summary>
/// Estados del buffer en el embudo de creacion
/// </summary>
public enum BufferStatus
{
    /// <summary>Usuario agregando fuentes</summary>
    Ingesta,

    /// <summary>Analizando y detectando conflictos</summary>
    Validando,

    /// <summary>Esperando resolucion de tensiones por usuario</summary>
    EsperandoResolucion,

    /// <summary>Ejecutando merge con Claude Code</summary>
    Fusionando,

    /// <summary>Universo generado exitosamente</summary>
    Completado,

    /// <summary>Error en alguna fase</summary>
    Error
}

/// <summary>
/// Entrada de fuente en el buffer con metadata
/// </summary>
public class SourceEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Tipo de fuente</summary>
    public SourceType Type { get; set; }

    /// <summary>
    /// Prioridad de la fuente (menor = mayor prioridad)
    /// 1: Prompt directo (intencion actual del usuario)
    /// 2: SQL/BD (estructura tecnica preexistente)
    /// 3: Archivos PDF/Docs (documentacion de referencia)
    /// 4: URLs (informacion externa, mas volatil)
    /// </summary>
    public int Priority { get; set; }

    /// <summary>Estado de procesamiento de la fuente</summary>
    public SourceStatus Status { get; set; } = SourceStatus.Pending;

    /// <summary>Mensaje de error si fallo el procesamiento</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Contenido original de la fuente</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Contenido procesado/parseado</summary>
    public string? ProcessedContent { get; set; }

    /// <summary>Metadata especifica segun el tipo de fuente</summary>
    public SourceMetadata Metadata { get; set; } = new();

    /// <summary>Entidades extraidas de esta fuente</summary>
    public List<ExtractedEntity> ExtractedEntities { get; set; } = new();

    /// <summary>Relaciones extraidas de esta fuente</summary>
    public List<ExtractedRelation> ExtractedRelations { get; set; } = new();

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}

/// <summary>
/// Tipos de fuentes soportadas
/// </summary>
public enum SourceType
{
    /// <summary>Texto escrito directamente por el usuario</summary>
    Prompt,

    /// <summary>Script SQL con CREATE TABLEs</summary>
    SQL,

    /// <summary>Archivo PDF con especificaciones</summary>
    PDF,

    /// <summary>Documento Word/texto</summary>
    Document,

    /// <summary>Excel con esquema de datos</summary>
    Excel,

    /// <summary>URL escaneada con Playwright</summary>
    URLScanner,

    /// <summary>Imagen analizada con vision</summary>
    Image,

    /// <summary>JSON de universo existente para merge</summary>
    ExistingUniverse
}

/// <summary>
/// Estado de procesamiento de una fuente
/// </summary>
public enum SourceStatus
{
    Pending,
    Processing,
    Processed,
    Error
}

/// <summary>
/// Metadata especifica de cada tipo de fuente
/// </summary>
public class SourceMetadata
{
    // Para SQL
    public string? DatabaseName { get; set; }
    public int? TablesCount { get; set; }

    // Para URL Scanner
    public string? OriginUrl { get; set; }
    public int? FormsDetected { get; set; }
    public int? TablesDetected { get; set; }
    public int? ApiResponsesDetected { get; set; }

    // Para archivos
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public long? FileSize { get; set; }

    // Para imagenes (vision summary)
    public string? VisionSummary { get; set; }

    // General
    public Dictionary<string, object> Extra { get; set; } = new();
}

/// <summary>
/// Entidad extraida de una fuente
/// </summary>
public class ExtractedEntity
{
    public string Name { get; set; } = string.Empty;
    public string? TableName { get; set; }
    public string SourceId { get; set; } = string.Empty;
    public List<ExtractedProperty> Properties { get; set; } = new();

    /// <summary>Confianza de la extraccion (0-1)</summary>
    public double Confidence { get; set; } = 1.0;
}

/// <summary>
/// Propiedad extraida de una entidad
/// </summary>
public class ExtractedProperty
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public bool IsRequired { get; set; }
    public bool IsUnique { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsForeignKey { get; set; }
    public string? ForeignKeyTarget { get; set; }
    public List<string>? AllowedValues { get; set; }
    public object? DefaultValue { get; set; }
}

/// <summary>
/// Relacion extraida entre entidades
/// </summary>
public class ExtractedRelation
{
    public string SourceEntity { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public string RelationType { get; set; } = "ManyToOne"; // OneToMany, ManyToOne, ManyToMany
    public string? ForeignKeyProperty { get; set; }
    public string SourceId { get; set; } = string.Empty;
}

/// <summary>
/// Reporte de conflicto entre fuentes
/// </summary>
public class ConflictReport
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Tipo de conflicto detectado</summary>
    public ConflictType Type { get; set; }

    /// <summary>Severidad del conflicto</summary>
    public ConflictSeverity Severity { get; set; }

    /// <summary>Descripcion legible del conflicto</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>ID de la primera fuente en conflicto</summary>
    public string SourceAId { get; set; } = string.Empty;

    /// <summary>Valor de la fuente A</summary>
    public string? SourceAValue { get; set; }

    /// <summary>ID de la segunda fuente en conflicto</summary>
    public string SourceBId { get; set; } = string.Empty;

    /// <summary>Valor de la fuente B</summary>
    public string? SourceBValue { get; set; }

    /// <summary>Entidad afectada por el conflicto</summary>
    public string? AffectedEntity { get; set; }

    /// <summary>Propiedad afectada por el conflicto</summary>
    public string? AffectedProperty { get; set; }

    /// <summary>Sugerencia de resolucion automatica</summary>
    public string? AutoResolutionSuggestion { get; set; }

    /// <summary>Si fue resuelto automaticamente</summary>
    public bool AutoResolved { get; set; }

    /// <summary>Valor final despues de resolucion</summary>
    public string? ResolvedValue { get; set; }

    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Tipos de conflictos
/// </summary>
public enum ConflictType
{
    /// <summary>Misma entidad con diferentes estructuras</summary>
    DuplicateEntityDifferentStructure,

    /// <summary>FK apunta a entidad inexistente</summary>
    BrokenForeignKey,

    /// <summary>Mismo campo con tipos incompatibles</summary>
    IncompatibleTypes,

    /// <summary>Nombres similares que podrian ser la misma entidad</summary>
    SimilarNames,

    /// <summary>Valores contradictorios para el mismo dato</summary>
    ContradictoryValues,

    /// <summary>Relacion definida de forma diferente</summary>
    ConflictingRelation
}

/// <summary>
/// Severidad del conflicto
/// </summary>
public enum ConflictSeverity
{
    /// <summary>Informativo, no bloquea</summary>
    Info,

    /// <summary>Advertencia, revisar pero no bloquea</summary>
    Warning,

    /// <summary>Error, debe resolverse antes del merge</summary>
    Error,

    /// <summary>Critico, genera Punto de Tension</summary>
    Critical
}

/// <summary>
/// Punto de tension que requiere decision del usuario
/// </summary>
public class TensionPoint
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Titulo del punto de tension</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Descripcion detallada del conflicto</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>ID del conflicto origen</summary>
    public string ConflictId { get; set; } = string.Empty;

    /// <summary>Opciones disponibles para el usuario</summary>
    public List<TensionOption> Options { get; set; } = new();

    /// <summary>Si ya fue resuelto por el usuario</summary>
    public bool IsResolved { get; set; }

    /// <summary>Opcion seleccionada por el usuario</summary>
    public string? SelectedOptionId { get; set; }

    /// <summary>Valor personalizado si eligio "otro"</summary>
    public string? CustomValue { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}

/// <summary>
/// Opcion para resolver un punto de tension
/// </summary>
public class TensionOption
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public bool IsRecommended { get; set; }
}

/// <summary>
/// Resumen de las fuentes en el buffer
/// </summary>
public class SourcesSummary
{
    public int TotalSources { get; set; }
    public Dictionary<string, int> ByType { get; set; } = new();
    public int TotalConflicts { get; set; }
    public int UnresolvedTensions { get; set; }
    public BufferStatus Status { get; set; }
}
