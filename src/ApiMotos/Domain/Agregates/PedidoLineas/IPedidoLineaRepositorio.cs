using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.PedidoLineas
{
    public interface IPedidoLineaRepositorio : IRepository<PedidoLinea, int>
    {
        public List<PedidoLinea> GetPedidoLineas(Spec<PedidoLinea> specification, int skip, int take);
        public List<PedidoLinea> GetPedidoLineas(Spec<PedidoLinea> specification);
        public Task<List<PedidoLinea>> GetPedidoLineasAsync(Spec<PedidoLinea> specification);
        public Task<List<PedidoLinea>> GetPedidoLineasAsync(Spec<PedidoLinea> specification, int skip, int take);
    }
}
