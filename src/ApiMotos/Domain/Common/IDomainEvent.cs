namespace ApiMotos.Domain.Common;

/// <summary>
/// Evento de dominio (Refactorización Ola 3, §4.4): un hecho que YA ocurrió en un
/// agregado. Es la pieza que resuelve efecto_cascada de forma desacoplada — el agregado
/// que lo emite no conoce a quién reacciona; la comunicación entre agregados va por
/// evento, no por navegación (frontera de agregado, A.1). Los publica IEventPublisher
/// DESPUÉS de persistir el agregado raíz.
/// </summary>
public interface IDomainEvent
{
}

/// <summary>
/// Evento GENÉRICO de transición de ciclo de vida: lo emite GenericTransicionHandler en
/// cada transición exitosa. Es el gatillo determinístico de las cascadas ("al devolver
/// un alquiler, la máquina vuelve a disponible"): una reacción se suscribe con
/// INotificationHandler&lt;DomainEventNotification&lt;TransicionRealizada&gt;&gt; y filtra
/// por Entidad + Accion — sin codegen por evento.
/// </summary>
public sealed record TransicionRealizada(
    string Entidad,
    int EntidadId,
    string Accion,
    string Desde,
    string Hacia) : IDomainEvent;
