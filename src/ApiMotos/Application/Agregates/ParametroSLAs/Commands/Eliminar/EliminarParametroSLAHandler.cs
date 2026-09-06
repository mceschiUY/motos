using ApiMotos.Domain.Agregates.ParametroSLAs;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.ParametroSLAs;

namespace ApiMotos.Application.Agregates.ParametroSLAs.Commands.Eliminar
{
    public class EliminarParametroSLAHandler : GenericEliminarHandler<ParametroSLA, EliminarParametroSLACommand>
    {
        public EliminarParametroSLAHandler(IParametroSLARepositorio repositorio, ParametroSLAHooks hooks)
            : base(repositorio, hooks, comando => comando.Id)
        {
        }
    }
}
