using MediatR;
using ApiMotos.Application.Agregates.Observaciones.Queries.Observaciones;

namespace ApiMotos.Application.Agregates.Observaciones.Queries.ByEnvioId
{
    public class ObservacionesByEnvioIdQuery : IRequest<List<ObservacionesDto>>
    {
        public int EnvioId { get; set; }
    }
}
