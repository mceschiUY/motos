using System.Collections.Concurrent;
using MediatR;
using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Domain.Common;

namespace ApiMotos.Infrastructure.Services;

/// <summary>
/// Adapter IN-PROCESS del puerto IEventPublisher (Refactorización Ola 3): envuelve cada
/// evento en DomainEventNotification&lt;T&gt; y lo publica por MediatR — las reacciones
/// corren en el MISMO request (y dentro de la transacción del caso de uso si el caller
/// usa IUnitOfWork.EnTransaccionAsync). Cuando aparezca integración externa, este adapter
/// se reemplaza por outbox sin tocar dominio ni handlers (§9 del plan).
/// </summary>
public sealed class MediatorEventPublisher : IEventPublisher
{
    private static readonly ConcurrentDictionary<Type, Type> TiposNotificacion = new();

    private readonly IMediator _mediator;

    public MediatorEventPublisher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task PublicarAsync(IDomainEvent evento, CancellationToken ct = default)
    {
        // DomainEventNotification<T> con el tipo REAL del evento (en compile-time acá solo
        // se ve IDomainEvent): así cada reacción se suscribe al evento concreto.
        var tipoNotificacion = TiposNotificacion.GetOrAdd(evento.GetType(),
            t => typeof(DomainEventNotification<>).MakeGenericType(t));
        var notificacion = (INotification)Activator.CreateInstance(tipoNotificacion, evento)!;
        return _mediator.Publish(notificacion, ct);
    }

    public async Task PublicarAsync(IEnumerable<IDomainEvent> eventos, CancellationToken ct = default)
    {
        foreach (var evento in eventos)
            await PublicarAsync(evento, ct);
    }
}
