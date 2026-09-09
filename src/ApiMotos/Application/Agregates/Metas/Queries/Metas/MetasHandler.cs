using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Metas.Queries.Metas
{
    public class MetasHandler : GenericListaHandler<MetasQuery, MetasDto>
    {
        private const string Sql = @"
SELECT e.*
    , vendedor.Nombre AS VendedorDisplay
FROM PC_METAS e
LEFT JOIN PC_VENDEDORES vendedor ON e.VendedorId = vendedor.Id";

        public MetasHandler(IQueryService consultas)
            : base(consultas, Sql, q => (q.Id, q.Skip, q.Take))
        {
        }
    }
}
