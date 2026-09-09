using FluentResults;
using NSpecifications;
using ApiMotos.Domain.Agregates.Vendedores;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Vendedores.Commands.Crear;
using ApiMotos.Application.Agregates.Vendedores.Commands.Modificar;

namespace ApiMotos.Application.Agregates.Vendedores
{
    /// <summary>
    /// Reglas de negocio de Vendedor (Etapa A). Las guardas intrínsecas (Nombre requerido,
    /// comisión 0..100) viven en el agregado (patrón MovimientoStock.Validar); acá va lo que
    /// necesita repositorio: unicidad de `Usuario` (un login del sitio → un solo vendedor).
    /// </summary>
    public class VendedorHooks : CrudHooks<Vendedor, CrearVendedorCommand, ModificarVendedorCommand>
    {
        private readonly IVendedorRepositorio _vendedorRepositorio;

        public VendedorHooks(IVendedorRepositorio pVendedorRepositorio, IReglasNegocioEjecutor motor)
            : base(motor, "Vendedor")
        {
            _vendedorRepositorio = pVendedorRepositorio;
        }

        public override Task<Result> AntesDeCrear(CrearVendedorCommand comando, CancellationToken ct)
            => ValidarUsuarioUnico(comando.Usuario, idExcluir: null);

        public override Task<Result> AntesDeModificar(ModificarVendedorCommand comando, Vendedor actual, CancellationToken ct)
            => ValidarUsuarioUnico(comando.Usuario, idExcluir: comando.Id);

        private async Task<Result> ValidarUsuarioUnico(string? usuario, int? idExcluir)
        {
            if (string.IsNullOrWhiteSpace(usuario)) return Result.Ok();
            var login = usuario.Trim().ToLower();
            var repetidos = await _vendedorRepositorio.GetVendedoresAsync(
                new Spec<Vendedor>(x => x.Usuario != null && x.Usuario.ToLower() == login
                                        && (idExcluir == null || x.Id != idExcluir.Value)));
            if (repetidos.Count > 0)
                return Result.Fail($"Ya existe un vendedor asociado al usuario '{usuario.Trim()}'");
            return Result.Ok();
        }
    }
}
