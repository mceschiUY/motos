using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.MovimientosStock.Queries.Existencias
{
    public record ExistenciasQuery(int? VarianteId = null, int? DepositoId = null) : IQuery<List<ExistenciasDto>>;
}
