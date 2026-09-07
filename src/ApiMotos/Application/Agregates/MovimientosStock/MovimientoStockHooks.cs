using ApiMotos.Domain.Agregates.MovimientosStock;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.MovimientosStock.Commands.Crear;
using ApiMotos.Application.Agregates.MovimientosStock.Commands.Modificar;

namespace ApiMotos.Application.Agregates.MovimientosStock
{
    // El Kardex no tiene reglas de unicidad: las validaciones (cantidad, tipo, consistencia de
    // transferencia) viven en el agregado MovimientoStock. Shell con el motor de reglas-dato.
    public class MovimientoStockHooks : CrudHooks<MovimientoStock, CrearMovimientoStockCommand, ModificarMovimientoStockCommand>
    {
        public MovimientoStockHooks(IReglasNegocioEjecutor motor)
            : base(motor, "MovimientoStock")
        {
        }
    }
}
