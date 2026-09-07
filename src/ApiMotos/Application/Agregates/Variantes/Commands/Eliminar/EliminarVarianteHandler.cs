using ApiMotos.Domain.Agregates.Variantes;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Variantes;

namespace ApiMotos.Application.Agregates.Variantes.Commands.Eliminar
{
    public class EliminarVarianteHandler : GenericEliminarHandler<Variante, EliminarVarianteCommand>
    {
        public EliminarVarianteHandler(IVarianteRepositorio repositorio, VarianteHooks hooks)
            : base(repositorio, hooks, comando => comando.Id)
        {
        }
    }
}
