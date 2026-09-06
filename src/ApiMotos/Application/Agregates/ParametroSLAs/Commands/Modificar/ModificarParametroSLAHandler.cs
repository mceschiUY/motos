using ApiMotos.Domain.Agregates.ParametroSLAs;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.ParametroSLAs;

namespace ApiMotos.Application.Agregates.ParametroSLAs.Commands.Modificar
{
    public class ModificarParametroSLAHandler : GenericModificarHandler<ParametroSLA, ModificarParametroSLACommand>
    {
        public ModificarParametroSLAHandler(IParametroSLARepositorio repositorio, ParametroSLAHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => comando.Id,
                (actual, comando) => actual.Modificar(actual.Etapa, comando.RangoAlertaUmbralAdvertenciaDias, comando.RangoAlertaLimiteDias))
        {
        }
    }
}
