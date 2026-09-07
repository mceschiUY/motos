using ApiMotos.Domain.Agregates.MovimientosStock;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.MovimientosStock;

namespace ApiMotos.Application.Agregates.MovimientosStock.Commands.Crear
{
    public class CrearMovimientoStockHandler : GenericCrearHandler<MovimientoStock, CrearMovimientoStockCommand>
    {
        public CrearMovimientoStockHandler(IMovimientoStockRepositorio repositorio, MovimientoStockHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => MovimientoStock.Crear(
                comando.VarianteId, comando.DepositoId, comando.DepositoDestinoId, comando.Tipo, comando.Cantidad,
                comando.CostoUnitario, comando.Motivo, comando.DocumentoOrigen, comando.Fecha, comando.Usuario))
        {
        }
    }
}
