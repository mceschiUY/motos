using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Clientes.Queries.Clientes;

namespace ApiMotos.Application.Agregates.Clientes.Queries.Clientes
{
    // Id: filtra en SQL (GetById O(1)); Skip/Take: paginación OFFSET/FETCH. Todos opcionales
    // (sin parámetros = lista completa, retrocompatible).
    public record ClientesQuery(int? Id = null, int? Skip = null, int? Take = null) : IQuery<List<ClientesDto>>;
}
