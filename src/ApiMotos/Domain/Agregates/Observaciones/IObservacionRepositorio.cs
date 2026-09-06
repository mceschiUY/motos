using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.Observaciones
{
    public interface IObservacionRepositorio : IRepository<Observacion, int>
    {
        public List<Observacion> GetObservaciones(Spec<Observacion> specification, int skip, int take);
        public List<Observacion> GetObservaciones(Spec<Observacion> specification);
        public Task<List<Observacion>> GetObservacionesAsync(Spec<Observacion> specification);
        public Task<List<Observacion>> GetObservacionesAsync(Spec<Observacion> specification, int skip, int take);
    }
}
