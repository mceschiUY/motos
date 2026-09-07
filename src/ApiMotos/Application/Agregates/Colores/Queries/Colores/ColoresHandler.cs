using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Colores.Queries.Colores
{
    public class ColoresHandler : GenericListaHandler<ColoresQuery, ColoresDto>
    {
        private const string Sql = @"
SELECT e.*
FROM PC_COLORES e";

        public ColoresHandler(IQueryService consultas)
            : base(consultas, Sql, q => (q.Id, q.Skip, q.Take))
        {
        }
    }
}
