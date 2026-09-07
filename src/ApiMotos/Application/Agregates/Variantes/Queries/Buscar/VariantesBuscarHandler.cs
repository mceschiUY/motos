using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Variantes.Queries.Variantes;

namespace ApiMotos.Application.Agregates.Variantes.Queries.Buscar
{
    public class VariantesBuscarHandler : GenericBuscarHandler<VariantesBuscarQuery, VariantesDto>
    {
        private const string Sql = @"SELECT TOP 10 * FROM PC_VARIANTES
                WHERE Sku LIKE @q OR CodigoBarras LIKE @q
                ORDER BY Id DESC";

        public VariantesBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
