namespace ApiMotos.Domain.Common;

/// <summary>
/// Puerto de SALIDA para eventos de dominio (Refactorización Ola 3, §4.3/§4.4). El
/// adapter actual es in-process (MediatorEventPublisher: reacciones en el mismo request
/// y la misma transacción); si aparece integración externa, el adapter cambia a outbox
/// SIN tocar dominio ni handlers (decisión §9 del plan: outbox recién cuando haga falta).
/// </summary>
public interface IEventPublisher
{
    Task PublicarAsync(IDomainEvent evento, CancellationToken ct = default);
    Task PublicarAsync(IEnumerable<IDomainEvent> eventos, CancellationToken ct = default);
}
