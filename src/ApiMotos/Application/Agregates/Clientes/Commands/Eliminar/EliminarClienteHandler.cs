using ApiMotos.Domain.Agregates.Clientes;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Clientes;

namespace ApiMotos.Application.Agregates.Clientes.Commands.Eliminar
{
    public class EliminarClienteHandler : GenericEliminarHandler<Cliente, EliminarClienteCommand>
    {
        public EliminarClienteHandler(IClienteRepositorio repositorio, ClienteHooks hooks)
            : base(repositorio, hooks, comando => comando.Id)
        {
        }
    }
}
