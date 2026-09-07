using ApiMotos.Domain.Agregates.Marcas;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Marcas;

namespace ApiMotos.Application.Agregates.Marcas.Commands.Crear
{
    public class CrearMarcaHandler : GenericCrearHandler<Marca, CrearMarcaCommand>
    {
        public CrearMarcaHandler(IMarcaRepositorio repositorio, MarcaHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => Marca.Crear(comando.Nombre, comando.Pais, comando.Activo))
        {
        }
    }
}
