using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Pedidos.Queries.Pedidos;

namespace ApiMotos.Application.Agregates.Pedidos.Queries.Buscar
{
    public class PedidosBuscarHandler : GenericBuscarHandler<PedidosBuscarQuery, PedidosDto>
    {
        private const string Sql = @"SELECT TOP 10 e.*
                , cliente.Nombre AS ClienteDisplay
                , vendedor.Nombre AS VendedorDisplay
                , deposito.Nombre AS DepositoDisplay
                , agencia.Nombre AS AgenciaDisplay
                , envio.CodigoRastreo AS EnvioDisplay
                , (SELECT COUNT(*) FROM PC_PEDIDO_LINEAS l WHERE l.PedidoId = e.Id) AS Lineas
                FROM PC_PEDIDOS e
                LEFT JOIN PC_CLIENTES cliente ON e.ClienteId = cliente.Id
                LEFT JOIN PC_VENDEDORES vendedor ON e.VendedorId = vendedor.Id
                LEFT JOIN PC_DEPOSITOS deposito ON e.DepositoId = deposito.Id
                LEFT JOIN PC_AGENCIAS agencia ON e.AgenciaId = agencia.Id
                LEFT JOIN PC_ENVIOS envio ON e.EnvioId = envio.Id
                WHERE e.Numero LIKE @q OR cliente.Nombre LIKE @q OR e.Estado LIKE @q
                ORDER BY e.Id DESC";

        public PedidosBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
