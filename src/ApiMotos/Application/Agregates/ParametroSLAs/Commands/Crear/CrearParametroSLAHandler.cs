using ApiMotos.Domain.Agregates.ParametroSLAs;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.ParametroSLAs;

namespace ApiMotos.Application.Agregates.ParametroSLAs.Commands.Crear
{
    public class CrearParametroSLAHandler : GenericCrearHandler<ParametroSLA, CrearParametroSLACommand>
    {
        public CrearParametroSLAHandler(IParametroSLARepositorio repositorio, ParametroSLAHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => ParametroSLA.Crear("facturacion", comando.RangoAlertaUmbralAdvertenciaDias, comando.RangoAlertaLimiteDias))
        {
        }
    }
}
