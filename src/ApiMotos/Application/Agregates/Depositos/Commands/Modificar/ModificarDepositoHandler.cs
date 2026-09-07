using ApiMotos.Domain.Agregates.Depositos;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Depositos;

namespace ApiMotos.Application.Agregates.Depositos.Commands.Modificar
{
    public class ModificarDepositoHandler : GenericModificarHandler<Deposito, ModificarDepositoCommand>
    {
        public ModificarDepositoHandler(IDepositoRepositorio repositorio, DepositoHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => comando.Id,
                (actual, comando) => actual.Modificar(comando.Codigo, comando.Nombre, comando.Direccion, comando.Activo))
        {
        }
    }
}
