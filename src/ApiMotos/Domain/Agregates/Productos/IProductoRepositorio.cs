using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.Productos
{
    public interface IProductoRepositorio : IRepository<Producto, int>
    {
        public List<Producto> GetProductos(Spec<Producto> specification, int skip, int take);
        public List<Producto> GetProductos(Spec<Producto> specification);
        public Task<List<Producto>> GetProductosAsync(Spec<Producto> specification);
        public Task<List<Producto>> GetProductosAsync(Spec<Producto> specification, int skip, int take);
    }
}
