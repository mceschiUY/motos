using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Actividades
{
    /// <summary>
    /// Actividad comercial (plan §3.3): el CRM mínimo. Cada contacto del vendedor con un
    /// cliente (visita, llamada, whatsapp, email) con su resultado y, si corresponde, la
    /// próxima acción que alimenta la agenda del vendedor.
    /// </summary>
    public class Actividad : BaseEntity<int>
    {
        public static readonly string[] TiposValidos = { "visita", "llamada", "whatsapp", "email" };
        public static readonly string[] ResultadosValidos = { "pedido", "sin_pedido", "reprogramar", "sin_contacto" };

        private Actividad() : base()
        {
            Tipo = string.Empty;
            Resultado = string.Empty;
            Fecha = Clock.Current.Now;
        }

        private Actividad(int pVendedorId, int pClienteId, string pTipo, DateTime pFecha, string pResultado,
            string? pNotas, DateTime? pProximaAccion, int? pPedidoId)
        {
            VendedorId = pVendedorId;
            ClienteId = pClienteId;
            Tipo = pTipo;
            Fecha = pFecha;
            Resultado = pResultado;
            Notas = pNotas;
            ProximaAccion = pProximaAccion;
            PedidoId = pPedidoId;
        }

        public int VendedorId { get; private set; }
        public int ClienteId { get; private set; }
        public string Tipo { get; private set; }
        public DateTime Fecha { get; private set; }
        public string Resultado { get; private set; }
        public string? Notas { get; private set; }
        /// <summary>Solo fecha (columna DATE): "volver el 15". Alimenta la agenda.</summary>
        public DateTime? ProximaAccion { get; private set; }
        /// <summary>Pedido que salió de este contacto (Etapa B). Obligatorio si Resultado = 'pedido'.</summary>
        public int? PedidoId { get; private set; }

        private static Result Validar(int vendedorId, int clienteId, string tipo, DateTime? fecha, string resultado,
            DateTime? proximaAccion, int? pedidoId)
        {
            if (vendedorId <= 0) return Result.Fail("Vendedor es requerido");
            if (clienteId <= 0) return Result.Fail("Cliente es requerido");
            if (string.IsNullOrWhiteSpace(tipo) || Array.IndexOf(TiposValidos, tipo) < 0)
                return Result.Fail("Tipo inválido (visita, llamada, whatsapp o email)");
            if (fecha is null || fecha == default(DateTime)) return Result.Fail("Fecha es requerida");
            if (string.IsNullOrWhiteSpace(resultado) || Array.IndexOf(ResultadosValidos, resultado) < 0)
                return Result.Fail("Resultado inválido (pedido, sin_pedido, reprogramar o sin_contacto)");
            if (resultado == "reprogramar" && proximaAccion is null)
                return Result.Fail("Si el resultado es 'reprogramar' hay que indicar la próxima acción");
            // Etapa B: si la visita terminó en pedido, hay que decir CUÁL (plan §3, reglas de
            // dominio). Que ese pedido exista lo verifica ActividadHooks (cross-entity).
            if (resultado == "pedido" && pedidoId is null or <= 0)
                return Result.Fail("Si el resultado es 'pedido' hay que indicar el pedido");
            return Result.Ok();
        }

        private static string? Limpiar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        public static Result<Actividad> Crear(int vendedorId, int clienteId, string tipo, DateTime? fecha, string resultado,
            string? notas, DateTime? proximaAccion, int? pedidoId)
        {
            var v = Validar(vendedorId, clienteId, tipo, fecha, resultado, proximaAccion, pedidoId);
            if (v.IsFailed) return Result.Fail<Actividad>(v.Errors);
            return new Actividad(vendedorId, clienteId, tipo, fecha!.Value, resultado, Limpiar(notas),
                proximaAccion?.Date, pedidoId is > 0 ? pedidoId : null);
        }

        public Result<Actividad> Modificar(int pVendedorId, int pClienteId, string pTipo, DateTime? pFecha, string pResultado,
            string? pNotas, DateTime? pProximaAccion, int? pPedidoId)
        {
            var v = Validar(pVendedorId, pClienteId, pTipo, pFecha, pResultado, pProximaAccion, pPedidoId);
            if (v.IsFailed) return Result.Fail<Actividad>(v.Errors);
            VendedorId = pVendedorId;
            ClienteId = pClienteId;
            Tipo = pTipo;
            Fecha = pFecha!.Value;
            Resultado = pResultado;
            Notas = Limpiar(pNotas);
            ProximaAccion = pProximaAccion?.Date;
            PedidoId = pPedidoId is > 0 ? pPedidoId : null;
            return this;
        }
    }
}
