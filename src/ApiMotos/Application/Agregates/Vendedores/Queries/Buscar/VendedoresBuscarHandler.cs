using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Vendedores.Queries.Vendedores;

namespace ApiMotos.Application.Agregates.Vendedores.Queries.Buscar
{
    public class VendedoresBuscarHandler : GenericBuscarHandler<VendedoresBuscarQuery, VendedoresDto>
    {
        private const string Sql = @"SELECT TOP 10 * FROM PC_VENDEDORES
                WHERE Nombre LIKE @q OR Zona LIKE @q OR Email LIKE @q OR Usuario LIKE @q
                ORDER BY Id DESC";

        public VendedoresBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
