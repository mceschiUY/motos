using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Vendedores.Queries.Vendedores;

namespace ApiMotos.Application.Agregates.Vendedores.Queries.Buscar
{
    public record VendedoresBuscarQuery(string Texto) : IQuery<List<VendedoresDto>>;
}
