using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.Clientes
{
    public interface IClienteRepositorio : IRepository<Cliente, int>
    {
        public List<Cliente> GetClientes(Spec<Cliente> specification, int skip, int take);
        public List<Cliente> GetClientes(Spec<Cliente> specification);
        public Task<List<Cliente>> GetClientesAsync(Spec<Cliente> specification);
        public Task<List<Cliente>> GetClientesAsync(Spec<Cliente> specification, int skip, int take);
    }
}
