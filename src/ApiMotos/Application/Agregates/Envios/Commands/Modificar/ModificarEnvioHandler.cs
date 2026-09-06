using ApiMotos.Domain.Agregates.Envios;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Envios;

namespace ApiMotos.Application.Agregates.Envios.Commands.Modificar
{
    public class ModificarEnvioHandler : GenericModificarHandler<Envio, ModificarEnvioCommand>
    {
        public ModificarEnvioHandler(IEnvioRepositorio repositorio, EnvioHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => comando.Id,
                (actual, comando) => actual.Modificar(comando.CodigoRastreo, actual.Estado, comando.FechaRecibido, comando.FechaFactura, comando.FechaEnvio, comando.FechaEntrega, comando.MotivoAnulacion, comando.ClienteId, comando.AgenciaId))
        {
        }
    }
}
