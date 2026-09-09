using ApiMotos.Domain.Agregates.Vendedores;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Vendedores;

namespace ApiMotos.Application.Agregates.Vendedores.Commands.Modificar
{
    public class ModificarVendedorHandler : GenericModificarHandler<Vendedor, ModificarVendedorCommand>
    {
        public ModificarVendedorHandler(IVendedorRepositorio repositorio, VendedorHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => comando.Id,
                (actual, comando) => actual.Modificar(
                    comando.Nombre, comando.Telefono, comando.Email, comando.Zona,
                    comando.ComisionPorcentaje, comando.Usuario, comando.Activo))
        {
        }
    }
}
