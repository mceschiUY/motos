using FluentResults;
using NSpecifications;
using ApiMotos.Domain.Agregates.Metas;
using ApiMotos.Domain.Agregates.Vendedores;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Metas.Commands.Crear;
using ApiMotos.Application.Agregates.Metas.Commands.Modificar;

namespace ApiMotos.Application.Agregates.Metas
{
    /// <summary>
    /// Reglas de negocio de Meta (Etapa A). Formato de Periodo y ObjetivoUsd > 0 viven en el
    /// agregado (Meta.Validar). Acá lo que necesita repositorio: el vendedor debe existir y
    /// solo puede haber UNA meta por vendedor y período (mensaje claro antes del índice único).
    /// </summary>
    public class MetaHooks : CrudHooks<Meta, CrearMetaCommand, ModificarMetaCommand>
    {
        private readonly IMetaRepositorio _metaRepositorio;
        private readonly IVendedorRepositorio _vendedorRepositorio;

        public MetaHooks(IMetaRepositorio pMetaRepositorio, IVendedorRepositorio pVendedorRepositorio, IReglasNegocioEjecutor motor)
            : base(motor, "Meta")
        {
            _metaRepositorio = pMetaRepositorio;
            _vendedorRepositorio = pVendedorRepositorio;
        }

        public override Task<Result> AntesDeCrear(CrearMetaCommand comando, CancellationToken ct)
            => Validar(comando.VendedorId, comando.Periodo, idExcluir: null);

        public override Task<Result> AntesDeModificar(ModificarMetaCommand comando, Meta actual, CancellationToken ct)
            => Validar(comando.VendedorId, comando.Periodo, idExcluir: comando.Id);

        private async Task<Result> Validar(int vendedorId, string periodo, int? idExcluir)
        {
            var vendedor = await _vendedorRepositorio.FindAsync(vendedorId);
            if (vendedor == null)
                return Result.Fail($"El vendedor {vendedorId} no existe");

            var p = (periodo ?? "").Trim();
            var repetidas = await _metaRepositorio.GetMetasAsync(
                new Spec<Meta>(x => x.VendedorId == vendedorId && x.Periodo == p
                                    && (idExcluir == null || x.Id != idExcluir.Value)));
            if (repetidas.Count > 0)
                return Result.Fail($"Ya existe una meta para {vendedor.Nombre} en el período {p}: modificala en vez de crear otra");
            return Result.Ok();
        }
    }
}
