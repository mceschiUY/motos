using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Variantes.Queries.Variantes;

namespace ApiMotos.Application.Agregates.Variantes.Queries.Buscar
{
    public record VariantesBuscarQuery(string Texto) : IQuery<List<VariantesDto>>;
}
