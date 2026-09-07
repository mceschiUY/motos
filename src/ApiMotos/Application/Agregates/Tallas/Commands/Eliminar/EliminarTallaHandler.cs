using ApiMotos.Domain.Agregates.Tallas;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Tallas;

namespace ApiMotos.Application.Agregates.Tallas.Commands.Eliminar
{
    public class EliminarTallaHandler : GenericEliminarHandler<Talla, EliminarTallaCommand>
    {
        public EliminarTallaHandler(ITallaRepositorio repositorio, TallaHooks hooks)
            : base(repositorio, hooks, comando => comando.Id)
        {
        }
    }
}
