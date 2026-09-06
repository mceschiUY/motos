using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.ParametroSLAs.Queries.ParametroSLAs;

namespace ApiMotos.Application.Agregates.ParametroSLAs.Queries.ParametroSLAs
{
    // Id: filtra en SQL (GetById O(1)); Skip/Take: paginación OFFSET/FETCH. Todos opcionales
    // (sin parámetros = lista completa, retrocompatible).
    public record ParametroSLAsQuery(int? Id = null, int? Skip = null, int? Take = null) : IQuery<List<ParametroSLAsDto>>;
}
