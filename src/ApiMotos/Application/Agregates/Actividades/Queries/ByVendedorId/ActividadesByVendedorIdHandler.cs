using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Actividades.Queries.Actividades;

namespace ApiMotos.Application.Agregates.Actividades.Queries.ByVendedorId
{
    public class ActividadesByVendedorIdHandler : GenericPorFkHandler<ActividadesByVendedorIdQuery, ActividadesDto>
    {
        private const string Sql = @"
SELECT e.*
    , vendedor.Nombre AS VendedorDisplay
    , cliente.Nombre AS ClienteDisplay
FROM PC_ACTIVIDADES e
LEFT JOIN PC_VENDEDORES vendedor ON e.VendedorId = vendedor.Id
LEFT JOIN PC_CLIENTES cliente ON e.ClienteId = cliente.Id
WHERE e.VendedorId = @VendedorId
ORDER BY e.Fecha DESC, e.Id DESC";

        public ActividadesByVendedorIdHandler(IQueryService consultas)
            : base(consultas, Sql, q => new { VendedorId = q.VendedorId })
        {
        }
    }
}
