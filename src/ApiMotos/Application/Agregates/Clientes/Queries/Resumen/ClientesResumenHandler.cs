using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Clientes.Queries.Resumen
{
    public class ClientesResumenHandler : GenericResumenHandler<ClientesResumenQuery, ClientesResumenDto, GrupoConteoCliente>
    {
        public ClientesResumenHandler(IQueryService consultas)
            : base(consultas,
                "SELECT COUNT(*) FROM PC_CLIENTES",
                null /* entidad sin campo de estado: PorEstado queda vacío */,
                null /* entidad sin campo fecha: PorMes queda vacío */,
                (total, porEstado, porMes) => new ClientesResumenDto { Total = total, PorEstado = porEstado, PorMes = porMes })
        {
        }
    }
}
