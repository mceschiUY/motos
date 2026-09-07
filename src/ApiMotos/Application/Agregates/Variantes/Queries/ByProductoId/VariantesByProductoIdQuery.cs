using MediatR;
using ApiMotos.Application.Agregates.Variantes.Queries.Variantes;

namespace ApiMotos.Application.Agregates.Variantes.Queries.ByProductoId
{
    public class VariantesByProductoIdQuery : IRequest<List<VariantesDto>>
    {
        public int ProductoId { get; set; }
    }
}
