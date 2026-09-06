namespace ApiMotos.Domain.Common;

/// <summary>
/// Puerto de Unit of Work (Refactorización Ola 2, §4.3): la frontera transaccional del
/// caso de uso. El CRUD simple no lo necesita (una operación = un SaveChanges en el
/// repositorio); existe para los casos MULTI-AGREGADO — el efecto cascada de la Ola 3
/// consume este puerto para que todas las escrituras del caso de uso confirmen juntas
/// o ninguna. El adapter (GeneratedUnitOfWork, sobre el contexto único generado) lo
/// emite el generador junto con GeneratedContext.
/// </summary>
public interface IUnitOfWork
{
    Task<int> ConfirmarAsync(CancellationToken ct = default);
    Task EnTransaccionAsync(Func<Task> operacion, CancellationToken ct = default);
}
