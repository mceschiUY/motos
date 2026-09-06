using ApiMotos.Domain.Agregates.Agencias;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Agencias;

namespace ApiMotos.Application.Agregates.Agencias.Commands.Eliminar
{
    public class EliminarAgenciaHandler : GenericEliminarHandler<Agencia, EliminarAgenciaCommand>
    {
        public EliminarAgenciaHandler(IAgenciaRepositorio repositorio, AgenciaHooks hooks)
            : base(repositorio, hooks, comando => comando.Id)
        {
        }
    }
}
