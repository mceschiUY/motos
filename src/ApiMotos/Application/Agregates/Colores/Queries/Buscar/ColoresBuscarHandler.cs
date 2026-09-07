using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Colores.Queries.Colores;

namespace ApiMotos.Application.Agregates.Colores.Queries.Buscar
{
    public class ColoresBuscarHandler : GenericBuscarHandler<ColoresBuscarQuery, ColoresDto>
    {
        private const string Sql = @"SELECT TOP 10 * FROM PC_COLORES
                WHERE Nombre LIKE @q
                ORDER BY Id DESC";

        public ColoresBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
