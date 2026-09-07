using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Tallas.Queries.Tallas;

namespace ApiMotos.Application.Agregates.Tallas.Queries.Buscar
{
    public class TallasBuscarHandler : GenericBuscarHandler<TallasBuscarQuery, TallasDto>
    {
        private const string Sql = @"SELECT TOP 10 * FROM PC_TALLAS
                WHERE Nombre LIKE @q
                ORDER BY Id DESC";

        public TallasBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
