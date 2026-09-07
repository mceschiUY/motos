using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Marcas.Queries.Marcas
{
    public class MarcasHandler : GenericListaHandler<MarcasQuery, MarcasDto>
    {
        private const string Sql = @"
SELECT e.*
FROM PC_MARCAS e";

        public MarcasHandler(IQueryService consultas)
            : base(consultas, Sql, q => (q.Id, q.Skip, q.Take))
        {
        }
    }
}
