using ApiMotos.Domain.Agregates.MovimientosStock;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.MovimientosStock;

namespace ApiMotos.Application.Agregates.MovimientosStock.Commands.Eliminar
{
    public class EliminarMovimientoStockHandler : GenericEliminarHandler<MovimientoStock, EliminarMovimientoStockCommand>
    {
        public EliminarMovimientoStockHandler(IMovimientoStockRepositorio repositorio, MovimientoStockHooks hooks)
            : base(repositorio, hooks, comando => comando.Id)
        {
        }
    }
}
