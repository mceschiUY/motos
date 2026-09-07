using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.Marcas
{
    public interface IMarcaRepositorio : IRepository<Marca, int>
    {
        public List<Marca> GetMarcas(Spec<Marca> specification, int skip, int take);
        public List<Marca> GetMarcas(Spec<Marca> specification);
        public Task<List<Marca>> GetMarcasAsync(Spec<Marca> specification);
        public Task<List<Marca>> GetMarcasAsync(Spec<Marca> specification, int skip, int take);
    }
}
