using ApiMotos.Domain.Agregates.Vendedores;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Vendedores;

namespace ApiMotos.Application.Agregates.Vendedores.Commands.Crear
{
    public class CrearVendedorHandler : GenericCrearHandler<Vendedor, CrearVendedorCommand>
    {
        public CrearVendedorHandler(IVendedorRepositorio repositorio, VendedorHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => Vendedor.Crear(
                comando.Nombre, comando.Telefono, comando.Email, comando.Zona,
                comando.ComisionPorcentaje, comando.Usuario, comando.Activo))
        {
        }
    }
}
