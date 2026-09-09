using ApiMotos.Domain.Agregates.Metas;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Metas;

namespace ApiMotos.Application.Agregates.Metas.Commands.Eliminar
{
    public class EliminarMetaHandler : GenericEliminarHandler<Meta, EliminarMetaCommand>
    {
        public EliminarMetaHandler(IMetaRepositorio repositorio, MetaHooks hooks)
            : base(repositorio, hooks, comando => comando.Id)
        {
        }
    }
}
