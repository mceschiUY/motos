using ApiMotos.Domain.Common;

namespace ApiMotos.Infrastructure.Services;

/// <summary>
/// Adapter de sistema del puerto IClock (Refactorización C.1). Se registra en Program.cs
/// como singleton y se asigna a Clock.Current (el ambiente que leen las guardas temporales
/// generadas en el dominio).
/// </summary>
public sealed class SystemClock : IClock
{
    public DateTime Now => DateTime.Now;
    public DateTime Today => DateTime.Today;
    public DateTime UtcNow => DateTime.UtcNow;
}
