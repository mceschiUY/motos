using MediatR;
using ApiMotos.Application.Agregates.Actividades.Queries.Actividades;

namespace ApiMotos.Application.Agregates.Actividades.Queries.ByClienteId
{
    public class ActividadesByClienteIdQuery : IRequest<List<ActividadesDto>>
    {
        public int ClienteId { get; set; }
    }
}
