namespace ApiMotos.Domain.Common
{
    public abstract class BaseEntity<TId>
    {
        public TId Id { get; set; }

        protected BaseEntity(TId pId)
        {
            Id = pId;
        }

        protected BaseEntity()
        {
        }

        // ─── Eventos de dominio (Refactorización Ola 3) ───
        // El agregado ACUMULA los hechos que produce; los generic handlers los drenan con
        // TomarEventos() y los publican por IEventPublisher DESPUÉS de persistir. La lista
        // no se mapea a EF (sin setter público, no es propiedad — EF la ignora).
        private readonly List<IDomainEvent> _eventos = new();

        /// <summary>El agregado registra un hecho de dominio (se publica al persistir).</summary>
        protected void Emitir(IDomainEvent evento) => _eventos.Add(evento);

        /// <summary>Drena los eventos pendientes (los devuelve y limpia la lista) —
        /// lo llama el handler DESPUÉS de guardar, para publicarlos exactamente una vez.</summary>
        public IReadOnlyList<IDomainEvent> TomarEventos()
        {
            if (_eventos.Count == 0) return Array.Empty<IDomainEvent>();
            var pendientes = _eventos.ToArray();
            _eventos.Clear();
            return pendientes;
        }
    }

}
