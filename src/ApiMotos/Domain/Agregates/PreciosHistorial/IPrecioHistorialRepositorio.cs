using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.PreciosHistorial
{
    public interface IPrecioHistorialRepositorio : IRepository<PrecioHistorial, int>
    {
        public Task<List<PrecioHistorial>> GetPreciosHistorialAsync(Spec<PrecioHistorial> specification);
    }
}
