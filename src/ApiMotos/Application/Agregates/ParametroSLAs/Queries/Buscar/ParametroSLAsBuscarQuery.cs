using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.ParametroSLAs.Queries.ParametroSLAs;

namespace ApiMotos.Application.Agregates.ParametroSLAs.Queries.Buscar
{
    public record ParametroSLAsBuscarQuery(string Texto) : IQuery<List<ParametroSLAsDto>>;
}
