using System.Security.Claims;
using FluentResults;
using NSpecifications;
using ApiMotos.Domain.Agregates.Variantes;
using ApiMotos.Domain.Agregates.PreciosHistorial;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Variantes.Commands.Crear;
using ApiMotos.Application.Agregates.Variantes.Commands.Modificar;

namespace ApiMotos.Application.Agregates.Variantes
{
    /// <summary>
    /// Reglas de la Variante (el SKU real): unicidad de Sku y de la combinación
    /// Producto × Talla × Color, más el <b>historial de precios</b> de la Etapa C (plan §3.8):
    /// cada PUT que cambia PrecioLista o CostoEstandar deja una fila en PC_PRECIO_HISTORIAL.
    ///
    /// Los valores viejos se capturan en AntesDeModificar (ahí `actual` todavía no se pisó) y se
    /// escriben en DespuesDeModificar, que corre después de persistir. El hook es Scoped, así
    /// que esos campos viven lo que dura el request.
    /// </summary>
    public class VarianteHooks : CrudHooks<Variante, CrearVarianteCommand, ModificarVarianteCommand>
    {
        private readonly IVarianteRepositorio _varianteRepositorio;
        private readonly IPrecioHistorialRepositorio _historial;
        private readonly IHttpContextAccessor _contexto;

        private decimal? _precioAnterior;
        private decimal? _costoAnterior;

        public VarianteHooks(IVarianteRepositorio pVarianteRepositorio, IPrecioHistorialRepositorio pHistorial,
            IHttpContextAccessor pContexto, IReglasNegocioEjecutor motor)
            : base(motor, "Variante")
        {
            _varianteRepositorio = pVarianteRepositorio;
            _historial = pHistorial;
            _contexto = pContexto;
        }

        /// <summary>
        /// Login del JWT, leído del claim como en <c>GET /api/Vendedor/mio</c> y NO por
        /// ICurrentUserService: en Development ese servicio es el mock y devuelve "dev_user",
        /// y el timeline tiene que mostrar quién tocó el precio de verdad.
        /// </summary>
        private string? UsuarioActual()
        {
            var user = _contexto.HttpContext?.User;
            return user?.FindFirstValue(ClaimTypes.Name) ?? user?.Identity?.Name;
        }

        public override async Task<Result> AntesDeCrear(CrearVarianteCommand comando, CancellationToken ct)
        {
            var skuRepetido = await _varianteRepositorio.GetVariantesAsync(
                new Spec<Variante>(x => x.Sku == comando.Sku));
            if (skuRepetido.Count > 0)
                return Result.Fail("Ya existe una variante con el mismo Sku");

            var comboRepetido = await _varianteRepositorio.GetVariantesAsync(
                new Spec<Variante>(x => x.ProductoId == comando.ProductoId && x.TallaId == comando.TallaId && x.ColorId == comando.ColorId));
            if (comboRepetido.Count > 0)
                return Result.Fail("Ya existe una variante con la misma combinación de Producto, Talla y Color");
            return Result.Ok();
        }

        public override async Task<Result> AntesDeModificar(ModificarVarianteCommand comando, Variante actual, CancellationToken ct)
        {
            var skuRepetido = await _varianteRepositorio.GetVariantesAsync(
                new Spec<Variante>(x => x.Sku == comando.Sku && x.Id != comando.Id));
            if (skuRepetido.Count > 0)
                return Result.Fail("Ya existe una variante con el mismo Sku");

            var comboRepetido = await _varianteRepositorio.GetVariantesAsync(
                new Spec<Variante>(x => x.ProductoId == comando.ProductoId && x.TallaId == comando.TallaId && x.ColorId == comando.ColorId && x.Id != comando.Id));
            if (comboRepetido.Count > 0)
                return Result.Fail("Ya existe una variante con la misma combinación de Producto, Talla y Color");

            // Foto de los valores viejos, antes de que Modificar los pise.
            _precioAnterior = actual.PrecioLista;
            _costoAnterior = actual.CostoEstandar;
            return Result.Ok();
        }

        /// <summary>
        /// Etapa C: una fila por campo que efectivamente cambió. Si el PUT manda el mismo precio
        /// no se historia nada (lo rechaza la factory) y el timeline no se llena de ruido.
        /// </summary>
        public override async Task<Result> DespuesDeModificar(ModificarVarianteCommand comando, Variante entidad, CancellationToken ct)
        {
            var ahora = Clock.Current.Now;
            var quien = UsuarioActual();

            if (_precioAnterior is decimal precioViejo && precioViejo != entidad.PrecioLista)
            {
                var fila = PrecioHistorial.Crear(entidad.Id, "precio", precioViejo, entidad.PrecioLista, ahora, quien);
                if (fila.IsSuccess) await _historial.AddAsync(fila.Value);
            }

            if (_costoAnterior is decimal costoViejo && costoViejo != entidad.CostoEstandar)
            {
                var fila = PrecioHistorial.Crear(entidad.Id, "costo", costoViejo, entidad.CostoEstandar, ahora, quien);
                if (fila.IsSuccess) await _historial.AddAsync(fila.Value);
            }

            _precioAnterior = null;
            _costoAnterior = null;
            return Result.Ok();
        }
    }
}
