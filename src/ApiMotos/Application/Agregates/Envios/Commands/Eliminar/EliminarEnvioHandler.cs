using ApiMotos.Domain.Agregates.Envios;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Envios;

namespace ApiMotos.Application.Agregates.Envios.Commands.Eliminar
{
    public class EliminarEnvioHandler : GenericEliminarHandler<Envio, EliminarEnvioCommand>
    {
        public EliminarEnvioHandler(IEnvioRepositorio repositorio, EnvioHooks hooks)
            : base(repositorio, hooks, comando => comando.Id)
        {
        }
    }
}
