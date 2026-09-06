using ApiMotos.Domain.Agregates.Clientes;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Clientes;

namespace ApiMotos.Application.Agregates.Clientes.Commands.Modificar
{
    public class ModificarClienteHandler : GenericModificarHandler<Cliente, ModificarClienteCommand>
    {
        public ModificarClienteHandler(IClienteRepositorio repositorio, ClienteHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => comando.Id,
                (actual, comando) => actual.Modificar(comando.Nombre, comando.Telefono, comando.DireccionEntrega))
        {
        }
    }
}
