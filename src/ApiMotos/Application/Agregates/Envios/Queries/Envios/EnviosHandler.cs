using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Envios.Queries.Envios
{
    public class EnviosHandler : GenericListaHandler<EnviosQuery, EnviosDto>
    {
        private const string Sql = @"
SELECT e.*
    , cliente.Nombre AS ClienteDisplay
    , agencia.Nombre AS AgenciaDisplay
    , pedido.Numero AS PedidoDisplay
FROM PC_ENVIOS e
LEFT JOIN PC_CLIENTES cliente ON e.ClienteId = cliente.Id
LEFT JOIN PC_AGENCIAS agencia ON e.AgenciaId = agencia.Id
LEFT JOIN PC_PEDIDOS pedido ON e.PedidoId = pedido.Id";

        public EnviosHandler(IQueryService consultas)
            : base(consultas, Sql, q => (q.Id, q.Skip, q.Take))
        {
        }
    }
}
