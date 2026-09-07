using ApiMotos.Domain.Agregates.Depositos;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Depositos;

namespace ApiMotos.Application.Agregates.Depositos.Commands.Eliminar
{
    public class EliminarDepositoHandler : GenericEliminarHandler<Deposito, EliminarDepositoCommand>
    {
        public EliminarDepositoHandler(IDepositoRepositorio repositorio, DepositoHooks hooks)
            : base(repositorio, hooks, comando => comando.Id)
        {
        }
    }
}
