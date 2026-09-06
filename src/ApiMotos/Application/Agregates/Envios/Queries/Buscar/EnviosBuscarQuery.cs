using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Envios.Queries.Envios;

namespace ApiMotos.Application.Agregates.Envios.Queries.Buscar
{
    public record EnviosBuscarQuery(string Texto) : IQuery<List<EnviosDto>>;
}
