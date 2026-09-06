using MediatR;
using ApiMotos.Application.Agregates.Envios.Queries.Envios;

namespace ApiMotos.Application.Agregates.Envios.Queries.ByClienteId
{
    public class EnviosByClienteIdQuery : IRequest<List<EnviosDto>>
    {
        public int ClienteId { get; set; }
    }
}
