using FluentResults;
using NSpecifications;
using ApiMotos.Domain.Agregates.Variantes;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Variantes.Commands.Crear;
using ApiMotos.Application.Agregates.Variantes.Commands.Modificar;

namespace ApiMotos.Application.Agregates.Variantes
{
    public class VarianteHooks : CrudHooks<Variante, CrearVarianteCommand, ModificarVarianteCommand>
    {
        private readonly IVarianteRepositorio _varianteRepositorio;

        public VarianteHooks(IVarianteRepositorio pVarianteRepositorio, IReglasNegocioEjecutor motor)
            : base(motor, "Variante")
        {
            _varianteRepositorio = pVarianteRepositorio;
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
            return Result.Ok();
        }
    }
}
