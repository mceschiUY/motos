using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.ParametroSLAs.Queries.ParametroSLAs
{
    public class ParametroSLAsHandler : GenericListaHandler<ParametroSLAsQuery, ParametroSLAsDto>
    {
        private const string Sql = @"
SELECT e.*
FROM PC_PARAMETROSLAS e";

        public ParametroSLAsHandler(IQueryService consultas)
            : base(consultas, Sql, q => (q.Id, q.Skip, q.Take))
        {
        }
    }
}
