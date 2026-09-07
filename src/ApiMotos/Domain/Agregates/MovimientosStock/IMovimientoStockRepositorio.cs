using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.MovimientosStock
{
    public interface IMovimientoStockRepositorio : IRepository<MovimientoStock, int>
    {
        public List<MovimientoStock> GetMovimientosStock(Spec<MovimientoStock> specification, int skip, int take);
        public List<MovimientoStock> GetMovimientosStock(Spec<MovimientoStock> specification);
        public Task<List<MovimientoStock>> GetMovimientosStockAsync(Spec<MovimientoStock> specification);
        public Task<List<MovimientoStock>> GetMovimientosStockAsync(Spec<MovimientoStock> specification, int skip, int take);
    }
}
