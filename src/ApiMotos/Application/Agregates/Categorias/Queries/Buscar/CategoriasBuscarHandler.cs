using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Categorias.Queries.Categorias;

namespace ApiMotos.Application.Agregates.Categorias.Queries.Buscar
{
    public class CategoriasBuscarHandler : GenericBuscarHandler<CategoriasBuscarQuery, CategoriasDto>
    {
        private const string Sql = @"SELECT TOP 10 * FROM PC_CATEGORIAS
                WHERE Nombre LIKE @q
                ORDER BY Id DESC";

        public CategoriasBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
