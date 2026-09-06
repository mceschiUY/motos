using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Envios.Queries.Envios;

namespace ApiMotos.Application.Agregates.Envios.Queries.Buscar
{
    public class EnviosBuscarHandler : GenericBuscarHandler<EnviosBuscarQuery, EnviosDto>
    {
        private const string Sql = @"SELECT TOP 10 * FROM PC_ENVIOS
                WHERE CodigoRastreo LIKE @q OR Estado LIKE @q OR MotivoAnulacion LIKE @q
                ORDER BY Id DESC";

        public EnviosBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
