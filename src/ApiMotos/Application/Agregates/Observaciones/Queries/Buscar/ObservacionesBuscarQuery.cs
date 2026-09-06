using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Observaciones.Queries.Observaciones;

namespace ApiMotos.Application.Agregates.Observaciones.Queries.Buscar
{
    public record ObservacionesBuscarQuery(string Texto) : IQuery<List<ObservacionesDto>>;
}
