using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Agencias.Queries.Agencias;

namespace ApiMotos.Application.Agregates.Agencias.Queries.Buscar
{
    public record AgenciasBuscarQuery(string Texto) : IQuery<List<AgenciasDto>>;
}
