using ApiMotos.Domain.Agregates.Variantes;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Variantes;

namespace ApiMotos.Application.Agregates.Variantes.Commands.Modificar
{
    public class ModificarVarianteHandler : GenericModificarHandler<Variante, ModificarVarianteCommand>
    {
        public ModificarVarianteHandler(IVarianteRepositorio repositorio, VarianteHooks hooks, IEventPublisher eventos)
            : base(repositorio, hooks, eventos, comando => comando.Id,
                (actual, comando) => actual.Modificar(
                    comando.ProductoId, comando.TallaId, comando.ColorId, comando.Sku, comando.CodigoBarras,
                    comando.CostoEstandar, comando.PrecioLista, comando.Activo))
        {
        }
    }
}
