using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Metas.Queries.Metas;

namespace ApiMotos.Application.Agregates.Metas.Queries.Buscar
{
    public record MetasBuscarQuery(string Texto) : IQuery<List<MetasDto>>;
}
