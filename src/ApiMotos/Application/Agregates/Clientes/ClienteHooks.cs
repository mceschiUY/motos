using FluentResults;
using NSpecifications;
using ApiMotos.Domain.Agregates.Clientes;
using ApiMotos.Domain.Agregates.Vendedores;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Clientes.Commands.Crear;
using ApiMotos.Application.Agregates.Clientes.Commands.Modificar;

namespace ApiMotos.Application.Agregates.Clientes
{
    /// <summary>
    /// Reglas de negocio de handler de Cliente: override de AntesDeCrear / DespuesDeCrear /
    /// AntesDeModificar / DespuesDeModificar / AntesDeEliminar / AntesDeAccion / DespuesDeAccion.
    /// Acá escribe forja-reglas las R-XXX no plantillables (unicidad compuesta, efectos
    /// cross-entity). Devolver Result.Fail("mensaje") corta el flujo → 400.
    /// NO tocar los *Handler.cs (shells regenerables) ni Application/Common/Generated.
    /// Etapa A: el enum `Tipo` lo valida el agregado (Cliente.TiposValidos); acá se exige que
    /// el `VendedorId` asignado exista (mensaje claro en vez del genérico de FK).
    /// </summary>
    public class ClienteHooks : CrudHooks<Cliente, CrearClienteCommand, ModificarClienteCommand>
    {
        private readonly IClienteRepositorio _clienteRepositorio;
        private readonly IVendedorRepositorio _vendedorRepositorio;

        public ClienteHooks(IClienteRepositorio pClienteRepositorio, IVendedorRepositorio pVendedorRepositorio, IReglasNegocioEjecutor motor)
            : base(motor, "Cliente")
        {
            _clienteRepositorio = pClienteRepositorio;
            _vendedorRepositorio = pVendedorRepositorio;
        }

        public override async Task<Result> AntesDeCrear(CrearClienteCommand comando, CancellationToken ct)
        {
            // R: unicidad de Nombre — el sistema IMPIDE el duplicado
            var repetidosNombre = await _clienteRepositorio.GetClientesAsync(
                new Spec<Cliente>(x => x.Nombre == comando.Nombre));
            if (repetidosNombre.Count > 0)
                return Result.Fail("Ya existe un registro con el mismo valor de Nombre");

            return await ValidarVendedor(comando.VendedorId);
        }

        public override async Task<Result> AntesDeModificar(ModificarClienteCommand comando, Cliente actual, CancellationToken ct)
        {
            // R: unicidad de Nombre — el sistema IMPIDE el duplicado
            var repetidosNombre = await _clienteRepositorio.GetClientesAsync(
                new Spec<Cliente>(x => x.Nombre == comando.Nombre && x.Id != comando.Id));
            if (repetidosNombre.Count > 0)
                return Result.Fail("Ya existe un registro con el mismo valor de Nombre");

            return await ValidarVendedor(comando.VendedorId);
        }

        private async Task<Result> ValidarVendedor(int? vendedorId)
        {
            if (vendedorId is null or <= 0) return Result.Ok();
            var vendedor = await _vendedorRepositorio.FindAsync(vendedorId.Value);
            if (vendedor == null)
                return Result.Fail($"El vendedor {vendedorId} no existe");
            return Result.Ok();
        }
    }
}
