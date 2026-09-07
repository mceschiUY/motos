using ApiMotos.Domain.Agregates.MovimientosStock;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.MovimientosStock;

namespace ApiMotos.Application.Agregates.MovimientosStock.Commands.Modificar
{
    public class ModificarMovimientoStockHandler : GenericModificarHandler<MovimientoStock, ModificarMovimientoStockCommand>
    {
        public ModificarMovimientoStockHandler(IMovimientoStockRepositorio repositorio, MovimientoStockHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => comando.Id,
                (actual, comando) => actual.Modificar(
                    comando.VarianteId, comando.DepositoId, comando.DepositoDestinoId, comando.Tipo, comando.Cantidad,
                    comando.CostoUnitario, comando.Motivo, comando.DocumentoOrigen, comando.Fecha, comando.Usuario))
        {
        }
    }
}
