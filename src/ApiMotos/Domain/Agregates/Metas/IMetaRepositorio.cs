using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.Metas
{
    public interface IMetaRepositorio : IRepository<Meta, int>
    {
        public List<Meta> GetMetas(Spec<Meta> specification, int skip, int take);
        public List<Meta> GetMetas(Spec<Meta> specification);
        public Task<List<Meta>> GetMetasAsync(Spec<Meta> specification);
        public Task<List<Meta>> GetMetasAsync(Spec<Meta> specification, int skip, int take);
    }
}
