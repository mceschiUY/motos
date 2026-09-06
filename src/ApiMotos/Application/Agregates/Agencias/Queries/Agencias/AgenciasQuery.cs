using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Agencias.Queries.Agencias;

namespace ApiMotos.Application.Agregates.Agencias.Queries.Agencias
{
    // Id: filtra en SQL (GetById O(1)); Skip/Take: paginación OFFSET/FETCH. Todos opcionales
    // (sin parámetros = lista completa, retrocompatible).
    public record AgenciasQuery(int? Id = null, int? Skip = null, int? Take = null) : IQuery<List<AgenciasDto>>;
}
