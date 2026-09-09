using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Metas.Queries.Metas;

namespace ApiMotos.Application.Agregates.Metas.Queries.Buscar
{
    public class MetasBuscarHandler : GenericBuscarHandler<MetasBuscarQuery, MetasDto>
    {
        private const string Sql = @"SELECT TOP 10 e.*, vendedor.Nombre AS VendedorDisplay
                FROM PC_METAS e
                LEFT JOIN PC_VENDEDORES vendedor ON e.VendedorId = vendedor.Id
                WHERE e.Periodo LIKE @q OR vendedor.Nombre LIKE @q
                ORDER BY e.Id DESC";

        public MetasBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
