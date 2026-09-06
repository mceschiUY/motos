using ApiMotos.Domain.Agregates.Clientes;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Clientes;

namespace ApiMotos.Application.Agregates.Clientes.Commands.Crear
{
    public class CrearClienteHandler : GenericCrearHandler<Cliente, CrearClienteCommand>
    {
        public CrearClienteHandler(IClienteRepositorio repositorio, ClienteHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => Cliente.Crear(comando.Nombre, comando.Telefono, comando.DireccionEntrega))
        {
        }
    }
}
