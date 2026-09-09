using MediatR;
using ApiMotos.Application.Agregates.Vendedores.Queries.Vendedores;

namespace ApiMotos.Application.Agregates.Vendedores.Queries.PorUsuario
{
    /// <summary>Vendedor asociado a un login del sitio (`PC_VENDEDORES.Usuario`), sin distinguir mayúsculas.</summary>
    public record VendedorPorUsuarioQuery(string Usuario) : IRequest<VendedoresDto?>;
}
