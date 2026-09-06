using MediatR;
using ApiMotos.Domain.Common;

namespace ApiMotos.Application.Common.Abstractions;

/// <summary>
/// Envoltura MediatR de un evento de dominio (Refactorización Ola 3): el dominio NO
/// conoce MediatR (IDomainEvent es POCO); el adapter (MediatorEventPublisher) envuelve
/// acá y publica. Una reacción de cascada se declara:
///   class AlDevolverAlquiler : INotificationHandler&lt;DomainEventNotification&lt;TransicionRealizada&gt;&gt;
/// y MediatR la descubre solo (RegisterServicesFromAssembly).
/// </summary>
public sealed record DomainEventNotification<TEvento>(TEvento Evento) : INotification
    where TEvento : IDomainEvent;
