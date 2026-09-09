using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Vendedores.Queries.Vendedores
{
    public class VendedoresHandler : GenericListaHandler<VendedoresQuery, VendedoresDto>
    {
        private const string Sql = @"
SELECT e.*
FROM PC_VENDEDORES e";

        public VendedoresHandler(IQueryService consultas)
            : base(consultas, Sql, q => (q.Id, q.Skip, q.Take))
        {
        }
    }
}
