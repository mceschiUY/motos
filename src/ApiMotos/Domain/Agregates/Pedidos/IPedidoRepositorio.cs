using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.Pedidos
{
    public interface IPedidoRepositorio : IRepository<Pedido, int>
    {
        public List<Pedido> GetPedidos(Spec<Pedido> specification, int skip, int take);
        public List<Pedido> GetPedidos(Spec<Pedido> specification);
        public Task<List<Pedido>> GetPedidosAsync(Spec<Pedido> specification);
        public Task<List<Pedido>> GetPedidosAsync(Spec<Pedido> specification, int skip, int take);
    }
}
