using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Metas.Queries.Metas;

namespace ApiMotos.Application.Agregates.Metas.Queries.ByVendedorId
{
    public class MetasByVendedorIdHandler : GenericPorFkHandler<MetasByVendedorIdQuery, MetasDto>
    {
        private const string Sql = @"
SELECT e.*
    , vendedor.Nombre AS VendedorDisplay
FROM PC_METAS e
LEFT JOIN PC_VENDEDORES vendedor ON e.VendedorId = vendedor.Id
WHERE e.VendedorId = @VendedorId
ORDER BY e.Periodo DESC";

        public MetasByVendedorIdHandler(IQueryService consultas)
            : base(consultas, Sql, q => new { VendedorId = q.VendedorId })
        {
        }
    }
}
