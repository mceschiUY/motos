using ApiMotos.Domain.Agregates.Envios;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Envios;

namespace ApiMotos.Application.Agregates.Envios.Commands.Crear
{
    public class CrearEnvioHandler : GenericCrearHandler<Envio, CrearEnvioCommand>
    {
        public CrearEnvioHandler(IEnvioRepositorio repositorio, EnvioHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => Envio.Crear(comando.CodigoRastreo, "recibido", comando.FechaRecibido, comando.FechaFactura, comando.FechaEnvio, comando.FechaEntrega, comando.MotivoAnulacion, comando.ClienteId, comando.AgenciaId))
        {
        }
    }
}
