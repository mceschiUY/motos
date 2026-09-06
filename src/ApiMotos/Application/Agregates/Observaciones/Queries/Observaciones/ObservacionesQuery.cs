using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Observaciones.Queries.Observaciones;

namespace ApiMotos.Application.Agregates.Observaciones.Queries.Observaciones
{
    // Id: filtra en SQL (GetById O(1)); Skip/Take: paginación OFFSET/FETCH. Todos opcionales
    // (sin parámetros = lista completa, retrocompatible).
    public record ObservacionesQuery(int? Id = null, int? Skip = null, int? Take = null) : IQuery<List<ObservacionesDto>>;
}
