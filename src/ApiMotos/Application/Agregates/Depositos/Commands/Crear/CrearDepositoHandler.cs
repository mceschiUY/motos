using ApiMotos.Domain.Agregates.Depositos;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Depositos;

namespace ApiMotos.Application.Agregates.Depositos.Commands.Crear
{
    public class CrearDepositoHandler : GenericCrearHandler<Deposito, CrearDepositoCommand>
    {
        public CrearDepositoHandler(IDepositoRepositorio repositorio, DepositoHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => Deposito.Crear(comando.Codigo, comando.Nombre, comando.Direccion, comando.Activo))
        {
        }
    }
}
