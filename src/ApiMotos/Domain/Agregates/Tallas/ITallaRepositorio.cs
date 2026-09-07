using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.Tallas
{
    public interface ITallaRepositorio : IRepository<Talla, int>
    {
        public List<Talla> GetTallas(Spec<Talla> specification, int skip, int take);
        public List<Talla> GetTallas(Spec<Talla> specification);
        public Task<List<Talla>> GetTallasAsync(Spec<Talla> specification);
        public Task<List<Talla>> GetTallasAsync(Spec<Talla> specification, int skip, int take);
    }
}
