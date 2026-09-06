using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Envios.Queries.Envios;

namespace ApiMotos.Application.Agregates.Envios.Queries.Envios
{
    // Id: filtra en SQL (GetById O(1)); Skip/Take: paginación OFFSET/FETCH. Todos opcionales
    // (sin parámetros = lista completa, retrocompatible).
    public record EnviosQuery(int? Id = null, int? Skip = null, int? Take = null) : IQuery<List<EnviosDto>>;
}
