namespace ApiMotos.Domain.Common;

/// <summary>
/// Puerto de reloj (Refactorización C.1): las reglas temporales del dominio dejan de leer
/// DateTime.Now directo — consumen Clock.Current. En producción es el reloj de sistema
/// (Infrastructure.Services.SystemClock, registrado en Program.cs); en un test de dominio
/// se pisa Clock.Current con un reloj FIJO y la regla temporal se vuelve determinística.
/// </summary>
public interface IClock
{
    DateTime Now { get; }
    DateTime Today { get; }
    DateTime UtcNow { get; }
}

/// <summary>
/// Holder ambiente del reloj. Es ambiente (y no constructor-inject) porque las guardas
/// temporales viven en métodos estáticos/instancia de la entidad invocados por reflection
/// (AggregateValidationBehavior) — no hay canal de DI hasta ahí. El default de sistema
/// vive acá (3 líneas) para que Domain no dependa de Infrastructure; el composition root
/// lo reemplaza por el adapter registrado.
/// </summary>
public static class Clock
{
    public static IClock Current { get; set; } = new RelojSistema();

    private sealed class RelojSistema : IClock
    {
        public DateTime Now => DateTime.Now;
        public DateTime Today => DateTime.Today;
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
