using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Tallas.Queries.Tallas
{
    public class TallasHandler : GenericListaHandler<TallasQuery, TallasDto>
    {
        private const string Sql = @"
SELECT e.*
FROM PC_TALLAS e";

        public TallasHandler(IQueryService consultas)
            : base(consultas, Sql, q => (q.Id, q.Skip, q.Take))
        {
        }
    }
}
