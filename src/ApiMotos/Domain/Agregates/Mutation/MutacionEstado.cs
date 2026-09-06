namespace ApiMotos.Domain.Agregates.Mutation;

/// <summary>
/// Estado del ciclo de vida de una mutación
/// </summary>
public enum MutacionEstado
{
    /// <summary>
    /// Mutación creada, pendiente de análisis
    /// </summary>
    Pendiente = 0,

    /// <summary>
    /// Análisis completado por Claude, esperando preview
    /// </summary>
    Analizada = 1,

    /// <summary>
    /// Preview generado, esperando confirmación del usuario
    /// </summary>
    Previsualizada = 2,

    /// <summary>
    /// Mutación ejecutada exitosamente, archivos escritos
    /// </summary>
    Ejecutada = 3,

    /// <summary>
    /// Error durante el análisis o ejecución
    /// </summary>
    Fallida = 4,

    /// <summary>
    /// Mutación revertida (rollback)
    /// </summary>
    Revertida = 5
}

/// <summary>
/// Capas de la arquitectura hexagonal afectadas
/// </summary>
public enum CapaArquitectura
{
    Domain = 0,
    Application = 1,
    Infrastructure = 2,
    Presentation = 3,
    Database = 4
}

/// <summary>
/// Tipo de impacto sobre un archivo
/// </summary>
public enum TipoImpacto
{
    Crear = 0,
    Modificar = 1,
    Eliminar = 2
}
