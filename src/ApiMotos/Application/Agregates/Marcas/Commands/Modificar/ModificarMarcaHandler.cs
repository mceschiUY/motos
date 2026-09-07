using ApiMotos.Domain.Agregates.Marcas;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Marcas;

namespace ApiMotos.Application.Agregates.Marcas.Commands.Modificar
{
    public class ModificarMarcaHandler : GenericModificarHandler<Marca, ModificarMarcaCommand>
    {
        public ModificarMarcaHandler(IMarcaRepositorio repositorio, MarcaHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => comando.Id,
                (actual, comando) => actual.Modificar(comando.Nombre, comando.Pais, comando.Activo))
        {
        }
    }
}
