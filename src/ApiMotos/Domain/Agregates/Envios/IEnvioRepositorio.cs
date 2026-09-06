using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.Envios
{
    public interface IEnvioRepositorio : IRepository<Envio, int>
    {
        public List<Envio> GetEnvios(Spec<Envio> specification, int skip, int take);
        public List<Envio> GetEnvios(Spec<Envio> specification);
        public Task<List<Envio>> GetEnviosAsync(Spec<Envio> specification);
        public Task<List<Envio>> GetEnviosAsync(Spec<Envio> specification, int skip, int take);
    }
}
