using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Marcas.Queries.Marcas;

namespace ApiMotos.Application.Agregates.Marcas.Queries.Buscar
{
    public class MarcasBuscarHandler : GenericBuscarHandler<MarcasBuscarQuery, MarcasDto>
    {
        private const string Sql = @"SELECT TOP 10 * FROM PC_MARCAS
                WHERE Nombre LIKE @q
                ORDER BY Id DESC";

        public MarcasBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
