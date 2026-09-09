using MediatR;
using ApiMotos.Application.Agregates.Variantes.Queries.Variantes;

namespace ApiMotos.Application.Agregates.Variantes.Queries.ByColorId
{
    public class VariantesByColorIdQuery : IRequest<List<VariantesDto>>
    {
        public int ColorId { get; set; }
    }
}
