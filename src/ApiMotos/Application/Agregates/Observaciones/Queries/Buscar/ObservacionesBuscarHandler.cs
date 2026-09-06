using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Observaciones.Queries.Observaciones;

namespace ApiMotos.Application.Agregates.Observaciones.Queries.Buscar
{
    public class ObservacionesBuscarHandler : GenericBuscarHandler<ObservacionesBuscarQuery, ObservacionesDto>
    {
        private const string Sql = @"SELECT TOP 10 * FROM PC_OBSERVACIONES
                WHERE Texto LIKE @q OR Usuario LIKE @q
                ORDER BY Id DESC";

        public ObservacionesBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
