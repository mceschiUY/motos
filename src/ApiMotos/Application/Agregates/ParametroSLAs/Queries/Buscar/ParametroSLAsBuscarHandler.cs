using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.ParametroSLAs.Queries.ParametroSLAs;

namespace ApiMotos.Application.Agregates.ParametroSLAs.Queries.Buscar
{
    public class ParametroSLAsBuscarHandler : GenericBuscarHandler<ParametroSLAsBuscarQuery, ParametroSLAsDto>
    {
        private const string Sql = @"SELECT TOP 10 * FROM PC_PARAMETROSLAS
                WHERE Etapa LIKE @q
                ORDER BY Id DESC";

        public ParametroSLAsBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
