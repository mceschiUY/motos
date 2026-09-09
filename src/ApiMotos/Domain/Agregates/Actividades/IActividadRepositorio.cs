using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.Actividades
{
    public interface IActividadRepositorio : IRepository<Actividad, int>
    {
        public List<Actividad> GetActividades(Spec<Actividad> specification, int skip, int take);
        public List<Actividad> GetActividades(Spec<Actividad> specification);
        public Task<List<Actividad>> GetActividadesAsync(Spec<Actividad> specification);
        public Task<List<Actividad>> GetActividadesAsync(Spec<Actividad> specification, int skip, int take);
    }
}
