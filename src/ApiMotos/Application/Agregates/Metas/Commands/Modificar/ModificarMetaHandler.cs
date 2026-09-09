using ApiMotos.Domain.Agregates.Metas;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Metas;

namespace ApiMotos.Application.Agregates.Metas.Commands.Modificar
{
    public class ModificarMetaHandler : GenericModificarHandler<Meta, ModificarMetaCommand>
    {
        public ModificarMetaHandler(IMetaRepositorio repositorio, MetaHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => comando.Id,
                (actual, comando) => actual.Modificar(comando.VendedorId, comando.Periodo, comando.ObjetivoUsd))
        {
        }
    }
}
