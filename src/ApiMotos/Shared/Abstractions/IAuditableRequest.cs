namespace ApiMotos.Shared.Abstractions;

/// <summary>
/// Interface marcadora para requests que deben ser auditados.
/// Implementar en Commands que modifican datos.
/// </summary>
public interface IAuditableRequest
{
    /// <summary>
    /// Obtiene el tipo de entidad afectada (ej: "Usuario", "Cliente", "Producto")
    /// </summary>
    string GetEntityType();

    /// <summary>
    /// Obtiene el ID de la entidad afectada (null para Create antes de persistir)
    /// </summary>
    string? GetEntityId();
}

/// <summary>
/// Interface para Commands que modifican datos y requieren auditoría completa.
/// Extiende IAuditableRequest con valores anteriores y nuevos.
/// </summary>
public interface IAuditableCommand : IAuditableRequest
{
    /// <summary>
    /// Obtiene los valores anteriores (para Update/Delete).
    /// Devolver null para Create.
    /// </summary>
    object? GetOldValues();

    /// <summary>
    /// Obtiene los valores nuevos (para Create/Update).
    /// Devolver null para Delete.
    /// </summary>
    object? GetNewValues();
}

/// <summary>
/// Clase base opcional para Commands auditables que facilita la implementación.
/// Los Commands pueden heredar de esta clase para obtener funcionalidad básica.
/// </summary>
public abstract class AuditableCommandBase : IAuditableCommand
{
    // Almacena valores originales cargados por el handler
    private object? _oldValues;

    public abstract string GetEntityType();

    public abstract string? GetEntityId();

    public virtual object? GetOldValues() => _oldValues;

    public virtual object? GetNewValues() => this;

    /// <summary>
    /// Establece los valores originales (llamar desde el handler antes de modificar)
    /// </summary>
    public void SetOldValues(object? oldValues)
    {
        _oldValues = oldValues;
    }
}
