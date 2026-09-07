using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.Depositos
{
    public interface IDepositoRepositorio : IRepository<Deposito, int>
    {
        public List<Deposito> GetDepositos(Spec<Deposito> specification, int skip, int take);
        public List<Deposito> GetDepositos(Spec<Deposito> specification);
        public Task<List<Deposito>> GetDepositosAsync(Spec<Deposito> specification);
        public Task<List<Deposito>> GetDepositosAsync(Spec<Deposito> specification, int skip, int take);
    }
}
