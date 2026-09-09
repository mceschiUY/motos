using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Pedidos
{
    /// <summary>
    /// Pedido de venta (plan §3.4). Ciclo: borrador → confirmado → preparado → despachado →
    /// entregado, con anulado como salida. La matriz de transiciones vive en ciclos-vida.json;
    /// los efectos (Kardex, Envío, comisión) en PedidoHooks.
    ///
    /// Lo que el usuario carga y lo que calcula el sistema están separados a propósito:
    /// Crear/Modificar solo tocan los datos de cabecera; Numero, TotalUsd, ComisionUsd y
    /// EnvioId son del sistema y se escriben por los métodos de abajo (nunca desde el form).
    /// Todo importe es USD (plan §7: solo dólares).
    /// </summary>
    public class Pedido : BaseEntity<int>
    {
        public static readonly string[] EstadosValidos =
            { "borrador", "confirmado", "preparado", "despachado", "entregado", "anulado" };

        /// <summary>Estados en los que el pedido todavía se puede editar (cabecera y líneas).</summary>
        public static readonly string[] EstadosEditables = { "borrador", "confirmado" };

        private Pedido() : base()
        {
            Numero = string.Empty;
            Estado = string.Empty;
            Fecha = Clock.Current.Now;
        }

        private Pedido(int pClienteId, int pVendedorId, int pDepositoId, int? pAgenciaId,
            DateTime pFecha, string? pObservaciones)
        {
            Numero = string.Empty;          // lo sella PedidoHooks.DespuesDeCrear: PED-000n
            Estado = "borrador";
            ClienteId = pClienteId;
            VendedorId = pVendedorId;
            DepositoId = pDepositoId;
            AgenciaId = pAgenciaId;
            Fecha = pFecha;
            Observaciones = pObservaciones;
            TotalUsd = 0m;
            ComisionUsd = 0m;
        }

        public string Numero { get; private set; }
        public DateTime Fecha { get; private set; }
        public int ClienteId { get; private set; }
        public int VendedorId { get; private set; }
        public int DepositoId { get; private set; }
        /// <summary>Agencia con la que se despacha. Se elige antes de despachar: la transición
        /// del ciclo no lleva cuerpo, así que el dato tiene que estar en el pedido.</summary>
        public int? AgenciaId { get; private set; }
        public string Estado { get; private set; }
        public decimal TotalUsd { get; private set; }
        public decimal ComisionUsd { get; private set; }
        public string? Observaciones { get; private set; }
        /// <summary>Envío generado al despachar (plan §3.6). Nulo hasta entonces.</summary>
        public int? EnvioId { get; private set; }
        public string? MotivoAnulacion { get; private set; }

        public bool EsEditable => Array.IndexOf(EstadosEditables, Estado) >= 0;

        private static Result Validar(int clienteId, int vendedorId, int depositoId, DateTime? fecha)
        {
            if (clienteId <= 0) return Result.Fail("Cliente es requerido");
            if (vendedorId <= 0) return Result.Fail("Vendedor es requerido");
            if (depositoId <= 0) return Result.Fail("Depósito es requerido");
            if (fecha is null || fecha == default(DateTime)) return Result.Fail("Fecha es requerida");
            return Result.Ok();
        }

        private static string? Limpiar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        public static Result<Pedido> Crear(int clienteId, int vendedorId, int depositoId, int? agenciaId,
            DateTime? fecha, string? observaciones)
        {
            var v = Validar(clienteId, vendedorId, depositoId, fecha);
            if (v.IsFailed) return Result.Fail<Pedido>(v.Errors);
            return new Pedido(clienteId, vendedorId, depositoId, agenciaId is > 0 ? agenciaId : null,
                fecha!.Value, Limpiar(observaciones));
        }

        public Result<Pedido> Modificar(int pClienteId, int pVendedorId, int pDepositoId, int? pAgenciaId,
            DateTime? pFecha, string? pObservaciones)
        {
            var v = Validar(pClienteId, pVendedorId, pDepositoId, pFecha);
            if (v.IsFailed) return Result.Fail<Pedido>(v.Errors);
            // El depósito manda el stock que se va a descontar: cambiarlo con el pedido ya
            // despachado dejaría el Kardex mintiendo. Cabecera congelada fuera de edición.
            if (!EsEditable && pDepositoId != DepositoId)
                return Result.Fail<Pedido>($"No se puede cambiar el depósito de un pedido {Estado}");
            ClienteId = pClienteId;
            VendedorId = pVendedorId;
            DepositoId = pDepositoId;
            AgenciaId = pAgenciaId is > 0 ? pAgenciaId : null;
            Fecha = pFecha!.Value;
            Observaciones = Limpiar(pObservaciones);
            return this;
        }

        // ─── Campos del sistema: los escriben los Hooks, nunca el form ───────────

        /// <summary>Sella el número definitivo (PED-000n). Solo una vez.</summary>
        public void SellarNumero(string numero)
        {
            if (!string.IsNullOrWhiteSpace(Numero)) return;
            Numero = numero;
        }

        /// <summary>Total = suma de los subtotales de las líneas. Lo recalcula PedidoLineaHooks.</summary>
        public void RecalcularTotal(decimal totalUsd) => TotalUsd = totalUsd < 0 ? 0m : totalUsd;

        /// <summary>Comisión del vendedor, fijada al entregar (plan §3.4).</summary>
        public void FijarComision(decimal comisionUsd) => ComisionUsd = comisionUsd < 0 ? 0m : comisionUsd;

        /// <summary>Enlaza el envío que nació al despachar.</summary>
        public void AsignarEnvio(int envioId) => EnvioId = envioId > 0 ? envioId : null;

        /// <summary>Deja registrado por qué se anuló (el estado lo mueve el ciclo).</summary>
        public void RegistrarMotivoAnulacion(string? motivo) => MotivoAnulacion = Limpiar(motivo);

        /// <summary>Deja un aviso en las observaciones sin pisar lo que escribió el vendedor.</summary>
        public void AgregarObservacion(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return;
            var nuevo = string.IsNullOrWhiteSpace(Observaciones) ? texto.Trim() : Observaciones + " · " + texto.Trim();
            Observaciones = nuevo.Length > 500 ? nuevo[..500] : nuevo;
        }
    }
}
