using ApiMotos.Domain.Agregates.Vendedores;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Vendedores;

namespace ApiMotos.Application.Agregates.Vendedores.Commands.Eliminar
{
    public class EliminarVendedorHandler : GenericEliminarHandler<Vendedor, EliminarVendedorCommand>
    {
        public EliminarVendedorHandler(IVendedorRepositorio repositorio, VendedorHooks hooks)
            : base(repositorio, hooks, comando => comando.Id)
        {
        }
    }
}
