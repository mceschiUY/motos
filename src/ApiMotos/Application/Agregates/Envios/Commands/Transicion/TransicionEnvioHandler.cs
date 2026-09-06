using ApiMotos.Domain.Agregates.Envios;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Envios;

namespace ApiMotos.Application.Agregates.Envios.Commands.Transicion
{
    /// <summary>
    /// UN handler para TODAS las transiciones de Envio (antes: un Command +
    /// Handler estampado POR transición). El libreto vive en GenericTransicionHandler:
    /// hooks (motor de reglas incluido) → matriz de ciclos-vida.json → asignar → guardar.
    /// </summary>
    public class TransicionEnvioHandler : GenericTransicionHandler<Envio, TransicionEnvioCommand>
    {
        public TransicionEnvioHandler(IEnvioRepositorio repositorio, EnvioHooks hooks, ICicloBanco banco, ApiMotos.Domain.Common.IEventPublisher eventos)
            : base(repositorio, hooks, banco, eventos, "Envio", comando => comando.Id, comando => comando.Accion)
        {
        }
    }
}
