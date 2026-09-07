using ApiMotos.Domain.Agregates.Marcas;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Marcas;

namespace ApiMotos.Application.Agregates.Marcas.Commands.Eliminar
{
    public class EliminarMarcaHandler : GenericEliminarHandler<Marca, EliminarMarcaCommand>
    {
        public EliminarMarcaHandler(IMarcaRepositorio repositorio, MarcaHooks hooks)
            : base(repositorio, hooks, comando => comando.Id)
        {
        }
    }
}
