using ApiMotos.Domain.Agregates.Tallas;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Tallas;

namespace ApiMotos.Application.Agregates.Tallas.Commands.Crear
{
    public class CrearTallaHandler : GenericCrearHandler<Talla, CrearTallaCommand>
    {
        public CrearTallaHandler(ITallaRepositorio repositorio, TallaHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => Talla.Crear(comando.Nombre, comando.Tipo, comando.Orden, comando.Activo))
        {
        }
    }
}
