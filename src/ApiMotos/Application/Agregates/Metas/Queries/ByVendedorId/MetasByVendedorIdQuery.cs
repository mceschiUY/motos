using MediatR;
using ApiMotos.Application.Agregates.Metas.Queries.Metas;

namespace ApiMotos.Application.Agregates.Metas.Queries.ByVendedorId
{
    public class MetasByVendedorIdQuery : IRequest<List<MetasDto>>
    {
        public int VendedorId { get; set; }
    }
}
