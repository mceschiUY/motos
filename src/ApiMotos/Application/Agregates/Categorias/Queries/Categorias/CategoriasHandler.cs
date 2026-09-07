using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Categorias.Queries.Categorias
{
    public class CategoriasHandler : GenericListaHandler<CategoriasQuery, CategoriasDto>
    {
        private const string Sql = @"
SELECT e.*
FROM PC_CATEGORIAS e";

        public CategoriasHandler(IQueryService consultas)
            : base(consultas, Sql, q => (q.Id, q.Skip, q.Take))
        {
        }
    }
}
