using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Actividades.Queries.Actividades;

namespace ApiMotos.Application.Agregates.Actividades.Queries.Buscar
{
    public class ActividadesBuscarHandler : GenericBuscarHandler<ActividadesBuscarQuery, ActividadesDto>
    {
        private const string Sql = @"SELECT TOP 10 e.*, vendedor.Nombre AS VendedorDisplay, cliente.Nombre AS ClienteDisplay
                FROM PC_ACTIVIDADES e
                LEFT JOIN PC_VENDEDORES vendedor ON e.VendedorId = vendedor.Id
                LEFT JOIN PC_CLIENTES cliente ON e.ClienteId = cliente.Id
                WHERE e.Notas LIKE @q OR cliente.Nombre LIKE @q OR vendedor.Nombre LIKE @q
                ORDER BY e.Id DESC";

        public ActividadesBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
