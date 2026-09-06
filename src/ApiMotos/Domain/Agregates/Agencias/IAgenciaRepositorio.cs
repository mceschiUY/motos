using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.Agencias
{
    public interface IAgenciaRepositorio : IRepository<Agencia, int>
    {
        public List<Agencia> GetAgencias(Spec<Agencia> specification, int skip, int take);
        public List<Agencia> GetAgencias(Spec<Agencia> specification);
        public Task<List<Agencia>> GetAgenciasAsync(Spec<Agencia> specification);
        public Task<List<Agencia>> GetAgenciasAsync(Spec<Agencia> specification, int skip, int take);
    }
}
