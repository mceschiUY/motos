using MediatR;
using ApiMotos.Application.Agregates.Actividades.Queries.Actividades;

namespace ApiMotos.Application.Agregates.Actividades.Queries.ByVendedorId
{
    public class ActividadesByVendedorIdQuery : IRequest<List<ActividadesDto>>
    {
        public int VendedorId { get; set; }
    }
}
