using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.Vendedores
{
    public interface IVendedorRepositorio : IRepository<Vendedor, int>
    {
        public List<Vendedor> GetVendedores(Spec<Vendedor> specification, int skip, int take);
        public List<Vendedor> GetVendedores(Spec<Vendedor> specification);
        public Task<List<Vendedor>> GetVendedoresAsync(Spec<Vendedor> specification);
        public Task<List<Vendedor>> GetVendedoresAsync(Spec<Vendedor> specification, int skip, int take);
    }
}
