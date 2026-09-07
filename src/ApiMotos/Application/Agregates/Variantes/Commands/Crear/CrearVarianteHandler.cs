using ApiMotos.Domain.Agregates.Variantes;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Variantes;

namespace ApiMotos.Application.Agregates.Variantes.Commands.Crear
{
    public class CrearVarianteHandler : GenericCrearHandler<Variante, CrearVarianteCommand>
    {
        public CrearVarianteHandler(IVarianteRepositorio repositorio, VarianteHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => Variante.Crear(
                comando.ProductoId, comando.TallaId, comando.ColorId, comando.Sku, comando.CodigoBarras,
                comando.CostoEstandar, comando.PrecioLista, comando.Activo))
        {
        }
    }
}
