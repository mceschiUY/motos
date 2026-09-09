using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Actividades.Queries.Actividades;

namespace ApiMotos.Application.Agregates.Actividades.Queries.ByClienteId
{
    public class ActividadesByClienteIdHandler : GenericPorFkHandler<ActividadesByClienteIdQuery, ActividadesDto>
    {
        private const string Sql = @"
SELECT e.*
    , vendedor.Nombre AS VendedorDisplay
    , cliente.Nombre AS ClienteDisplay
FROM PC_ACTIVIDADES e
LEFT JOIN PC_VENDEDORES vendedor ON e.VendedorId = vendedor.Id
LEFT JOIN PC_CLIENTES cliente ON e.ClienteId = cliente.Id
WHERE e.ClienteId = @ClienteId
ORDER BY e.Fecha DESC, e.Id DESC";

        public ActividadesByClienteIdHandler(IQueryService consultas)
            : base(consultas, Sql, q => new { ClienteId = q.ClienteId })
        {
        }
    }
}
