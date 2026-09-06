using MediatR;
using ApiMotos.Application.Agregates.Envios.Queries.Envios;

namespace ApiMotos.Application.Agregates.Envios.Queries.ByAgenciaId
{
    public class EnviosByAgenciaIdQuery : IRequest<List<EnviosDto>>
    {
        public int AgenciaId { get; set; }
    }
}
