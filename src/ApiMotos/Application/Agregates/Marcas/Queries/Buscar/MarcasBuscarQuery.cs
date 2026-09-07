using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Marcas.Queries.Marcas;

namespace ApiMotos.Application.Agregates.Marcas.Queries.Buscar
{
    public record MarcasBuscarQuery(string Texto) : IQuery<List<MarcasDto>>;
}
