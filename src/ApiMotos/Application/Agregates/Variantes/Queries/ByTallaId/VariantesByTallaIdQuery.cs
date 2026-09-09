using MediatR;
using ApiMotos.Application.Agregates.Variantes.Queries.Variantes;

namespace ApiMotos.Application.Agregates.Variantes.Queries.ByTallaId
{
    public class VariantesByTallaIdQuery : IRequest<List<VariantesDto>>
    {
        public int TallaId { get; set; }
    }
}
