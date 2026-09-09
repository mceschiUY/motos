using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Actividades.Queries.Actividades
{
    public class ActividadesHandler : GenericListaHandler<ActividadesQuery, ActividadesDto>
    {
        private const string Sql = @"
SELECT e.*
    , vendedor.Nombre AS VendedorDisplay
    , cliente.Nombre AS ClienteDisplay
FROM PC_ACTIVIDADES e
LEFT JOIN PC_VENDEDORES vendedor ON e.VendedorId = vendedor.Id
LEFT JOIN PC_CLIENTES cliente ON e.ClienteId = cliente.Id";

        public ActividadesHandler(IQueryService consultas)
            : base(consultas, Sql, q => (q.Id, q.Skip, q.Take))
        {
        }
    }
}
