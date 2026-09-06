namespace ApiMotos.Domain.Specifications;

/// <summary>
/// SmartEnum Pattern - Codigos de error tipados
/// Ventajas sobre strings magicos:
/// - Intellisense y autocompletado
/// - Refactoring seguro
/// - El Frontend puede usar el Code (int) para logica
/// - Agrupacion por categoria (ranges de numeros)
/// </summary>
public sealed class DomainErrorCode
{
    public int Code { get; }
    public string Name { get; }
    public ErrorCategory Category { get; }
    public string DefaultMessage { get; }

    private DomainErrorCode(int code, string name, ErrorCategory category, string defaultMessage)
    {
        Code = code;
        Name = name;
        Category = category;
        DefaultMessage = defaultMessage;
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // ERRORES DE ESTADO (1000-1999)
    // Cuando la entidad no esta en el estado correcto para la operacion
    // ═══════════════════════════════════════════════════════════════════════════════

    public static readonly DomainErrorCode InvalidState =
        new(1000, "INVALID_STATE", ErrorCategory.State, "La entidad no esta en un estado valido para esta operacion");

    public static readonly DomainErrorCode MustBeDraft =
        new(1001, "MUST_BE_DRAFT", ErrorCategory.State, "La entidad debe estar en estado Borrador");

    public static readonly DomainErrorCode MustBeIssued =
        new(1002, "MUST_BE_ISSUED", ErrorCategory.State, "La entidad debe estar en estado Emitida");

    public static readonly DomainErrorCode MustBeActive =
        new(1003, "MUST_BE_ACTIVE", ErrorCategory.State, "La entidad debe estar activa");

    public static readonly DomainErrorCode AlreadyProcessed =
        new(1004, "ALREADY_PROCESSED", ErrorCategory.State, "La entidad ya fue procesada");

    public static readonly DomainErrorCode CannotBeDeleted =
        new(1005, "CANNOT_BE_DELETED", ErrorCategory.State, "La entidad no puede ser eliminada en su estado actual");

    // ═══════════════════════════════════════════════════════════════════════════════
    // ERRORES TEMPORALES (2000-2999)
    // Cuando hay restricciones de tiempo
    // ═══════════════════════════════════════════════════════════════════════════════

    public static readonly DomainErrorCode TooSoon =
        new(2000, "TOO_SOON", ErrorCategory.Temporal, "Debe esperar mas tiempo antes de realizar esta operacion");

    public static readonly DomainErrorCode TooLate =
        new(2001, "TOO_LATE", ErrorCategory.Temporal, "El tiempo para realizar esta operacion ha expirado");

    public static readonly DomainErrorCode WithinGracePeriod =
        new(2002, "WITHIN_GRACE_PERIOD", ErrorCategory.Temporal, "Aun esta dentro del periodo de gracia");

    public static readonly DomainErrorCode OutsideAllowedWindow =
        new(2003, "OUTSIDE_ALLOWED_WINDOW", ErrorCategory.Temporal, "Fuera de la ventana de tiempo permitida");

    // ═══════════════════════════════════════════════════════════════════════════════
    // ERRORES DE DEPENDENCIA (3000-3999)
    // Cuando hay relaciones que impiden la operacion
    // ═══════════════════════════════════════════════════════════════════════════════

    public static readonly DomainErrorCode HasDependencies =
        new(3000, "HAS_DEPENDENCIES", ErrorCategory.Dependency, "La entidad tiene dependencias que impiden la operacion");

    public static readonly DomainErrorCode RelatedEntityNotFound =
        new(3001, "RELATED_ENTITY_NOT_FOUND", ErrorCategory.Dependency, "No se encontro la entidad relacionada");

    public static readonly DomainErrorCode RelatedEntityInactive =
        new(3002, "RELATED_ENTITY_INACTIVE", ErrorCategory.Dependency, "La entidad relacionada no esta activa");

    public static readonly DomainErrorCode HasPendingItems =
        new(3003, "HAS_PENDING_ITEMS", ErrorCategory.Dependency, "Tiene items pendientes asociados");

    public static readonly DomainErrorCode ReferentialIntegrity =
        new(3004, "REFERENTIAL_INTEGRITY", ErrorCategory.Dependency, "Violacion de integridad referencial");

    // ═══════════════════════════════════════════════════════════════════════════════
    // ERRORES DE VALOR/ATRIBUTO (4000-4999)
    // Cuando un valor no cumple las reglas de negocio
    // ═══════════════════════════════════════════════════════════════════════════════

    public static readonly DomainErrorCode InvalidValue =
        new(4000, "INVALID_VALUE", ErrorCategory.Attribute, "El valor no es valido");

    public static readonly DomainErrorCode ValueOutOfRange =
        new(4001, "VALUE_OUT_OF_RANGE", ErrorCategory.Attribute, "El valor esta fuera del rango permitido");

    public static readonly DomainErrorCode ValueTooLow =
        new(4002, "VALUE_TOO_LOW", ErrorCategory.Attribute, "El valor es demasiado bajo");

    public static readonly DomainErrorCode ValueTooHigh =
        new(4003, "VALUE_TOO_HIGH", ErrorCategory.Attribute, "El valor es demasiado alto");

    public static readonly DomainErrorCode InsufficientBalance =
        new(4004, "INSUFFICIENT_BALANCE", ErrorCategory.Attribute, "Saldo insuficiente");

    public static readonly DomainErrorCode InsufficientStock =
        new(4005, "INSUFFICIENT_STOCK", ErrorCategory.Attribute, "Stock insuficiente");

    // ═══════════════════════════════════════════════════════════════════════════════
    // ERRORES DE NEGOCIO GENERICOS (5000-5999)
    // ═══════════════════════════════════════════════════════════════════════════════

    public static readonly DomainErrorCode BusinessRuleViolation =
        new(5000, "BUSINESS_RULE_VIOLATION", ErrorCategory.Business, "Violacion de regla de negocio");

    public static readonly DomainErrorCode OperationNotAllowed =
        new(5001, "OPERATION_NOT_ALLOWED", ErrorCategory.Business, "Operacion no permitida");

    public static readonly DomainErrorCode PreconditionFailed =
        new(5002, "PRECONDITION_FAILED", ErrorCategory.Business, "No se cumplen las precondiciones");

    // ═══════════════════════════════════════════════════════════════════════════════
    // Metodos de utilidad
    // ═══════════════════════════════════════════════════════════════════════════════

    public override string ToString() => $"[{Code}] {Name}";

    public override bool Equals(object? obj) => obj is DomainErrorCode other && Code == other.Code;

    public override int GetHashCode() => Code.GetHashCode();

    public static bool operator ==(DomainErrorCode? left, DomainErrorCode? right)
        => left?.Code == right?.Code;

    public static bool operator !=(DomainErrorCode? left, DomainErrorCode? right)
        => !(left == right);

    /// <summary>
    /// Busca un error por codigo numerico
    /// </summary>
    public static DomainErrorCode? FromCode(int code)
    {
        return code switch
        {
            1000 => InvalidState,
            1001 => MustBeDraft,
            1002 => MustBeIssued,
            1003 => MustBeActive,
            1004 => AlreadyProcessed,
            1005 => CannotBeDeleted,
            2000 => TooSoon,
            2001 => TooLate,
            2002 => WithinGracePeriod,
            2003 => OutsideAllowedWindow,
            3000 => HasDependencies,
            3001 => RelatedEntityNotFound,
            3002 => RelatedEntityInactive,
            3003 => HasPendingItems,
            3004 => ReferentialIntegrity,
            4000 => InvalidValue,
            4001 => ValueOutOfRange,
            4002 => ValueTooLow,
            4003 => ValueTooHigh,
            4004 => InsufficientBalance,
            4005 => InsufficientStock,
            5000 => BusinessRuleViolation,
            5001 => OperationNotAllowed,
            5002 => PreconditionFailed,
            _ => null
        };
    }
}

/// <summary>
/// Categorias de errores para agrupacion y filtrado
/// </summary>
public enum ErrorCategory
{
    /// <summary>Errores de estado de la entidad</summary>
    State = 1,

    /// <summary>Errores relacionados con tiempo</summary>
    Temporal = 2,

    /// <summary>Errores de dependencias/relaciones</summary>
    Dependency = 3,

    /// <summary>Errores de valores/atributos</summary>
    Attribute = 4,

    /// <summary>Errores de reglas de negocio genericas</summary>
    Business = 5
}
