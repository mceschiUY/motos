using ApiMotos.Domain.Agregates.Metas;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Metas;

namespace ApiMotos.Application.Agregates.Metas.Commands.Crear
{
    public class CrearMetaHandler : GenericCrearHandler<Meta, CrearMetaCommand>
    {
        public CrearMetaHandler(IMetaRepositorio repositorio, MetaHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => Meta.Crear(comando.VendedorId, comando.Periodo, comando.ObjetivoUsd))
        {
        }
    }
}
