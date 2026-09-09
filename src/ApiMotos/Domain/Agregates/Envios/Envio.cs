using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Envios
{
    public class Envio : BaseEntity<int>
    {
        private Envio() : base()
        {
            CodigoRastreo = string.Empty;
            Estado = string.Empty;
            FechaRecibido = Clock.Current.Now;
            FechaFactura = Clock.Current.Now;
            FechaEnvio = Clock.Current.Now;
            FechaEntrega = Clock.Current.Now;
            MotivoAnulacion = string.Empty;
            ClienteId = 0;
            AgenciaId = 0;
        }

        private Envio(string pCodigoRastreo, string pEstado, DateTime pFechaRecibido, DateTime pFechaFactura, DateTime pFechaEnvio, DateTime pFechaEntrega, string pMotivoAnulacion, int pClienteId, int pAgenciaId, int? pPedidoId)
        {
            PedidoId = pPedidoId;
            CodigoRastreo = pCodigoRastreo;
            Estado = pEstado;
            FechaRecibido = pFechaRecibido;
            FechaFactura = pFechaFactura;
            FechaEnvio = pFechaEnvio;
            FechaEntrega = pFechaEntrega;
            MotivoAnulacion = pMotivoAnulacion;
            ClienteId = pClienteId;
            AgenciaId = pAgenciaId;
        }

        public string CodigoRastreo { get; private set; }
        public string Estado { get; private set; }
        public DateTime FechaRecibido { get; private set; }
        public DateTime FechaFactura { get; private set; }
        public DateTime FechaEnvio { get; private set; }
        public DateTime FechaEntrega { get; private set; }
        public string MotivoAnulacion { get; private set; }
        public int ClienteId { get; private set; }
        public int AgenciaId { get; private set; }
        /// <summary>Pedido que originó el envío (Etapa B, plan §3.6). Nulo en los envíos sueltos.</summary>
        public int? PedidoId { get; private set; }

        public static Result<Envio> Crear(string codigoRastreo, string estado, DateTime fechaRecibido, DateTime fechaFactura, DateTime fechaEnvio, DateTime fechaEntrega, string motivoAnulacion, int clienteId, int agenciaId, int? pedidoId = null)
        {
            if (string.IsNullOrWhiteSpace(codigoRastreo)) return Result.Fail<Envio>("CodigoRastreo es requerido");
            if (string.IsNullOrWhiteSpace(estado)) return Result.Fail<Envio>("Estado es requerido");
            return new Envio(codigoRastreo, estado, fechaRecibido, fechaFactura, fechaEnvio, fechaEntrega, motivoAnulacion, clienteId, agenciaId, pedidoId is > 0 ? pedidoId : null);
        }

        public Result<Envio> Modificar(string pCodigoRastreo, string pEstado, DateTime pFechaRecibido, DateTime pFechaFactura, DateTime pFechaEnvio, DateTime pFechaEntrega, string pMotivoAnulacion, int pClienteId, int pAgenciaId)
        {
            if (string.IsNullOrWhiteSpace(pCodigoRastreo)) return Result.Fail<Envio>("CodigoRastreo es requerido");
            if (string.IsNullOrWhiteSpace(pEstado)) return Result.Fail<Envio>("Estado es requerido");
            CodigoRastreo = pCodigoRastreo;
            Estado = pEstado;
            FechaRecibido = pFechaRecibido;
            FechaFactura = pFechaFactura;
            FechaEnvio = pFechaEnvio;
            FechaEntrega = pFechaEntrega;
            MotivoAnulacion = pMotivoAnulacion;
            ClienteId = pClienteId;
            AgenciaId = pAgenciaId;
            return this;
        }

        /// <summary>Enlaza el envío con el pedido que lo originó (lo llama PedidoHooks al despachar).</summary>
        public void AsignarPedido(int pedidoId) => PedidoId = pedidoId > 0 ? pedidoId : null;

        // ═══════════════════════════════════════════════════════════════════════════════
        // SISTEMA DE VALIDACIÓN UNIVERSAL - Métodos de validación de capabilities
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Valida si se puede ejecutar una acción específica
        /// </summary>
        public Task<ApiMotos.Application.Common.Validation.BusinessValidationResult> PuedeEjecutarAsync(string actionKey, CancellationToken ct = default)
        {
            return actionKey.ToLower() switch
            {
                "crear" or "create" => PuedeCrearAsync(ct),
                "modificar" or "update" => PuedeModificarAsync(ct),
                "eliminar" or "delete" => PuedeEliminarAsync(ct),
                _ => Task.FromResult(ApiMotos.Application.Common.Validation.BusinessValidationResult.Success())
            };
        }

        /// <summary>
        /// Valida si se puede crear una instancia de Envio
        /// </summary>
        public static Task<ApiMotos.Application.Common.Validation.BusinessValidationResult> PuedeCrearAsync(CancellationToken ct = default)
        {
            // Sin validadores definidos - siempre permitido
            return Task.FromResult(ApiMotos.Application.Common.Validation.BusinessValidationResult.Success());
        }

        /// <summary>
        /// Valida si se puede modificar esta instancia de Envio
        /// </summary>
        public Task<ApiMotos.Application.Common.Validation.BusinessValidationResult> PuedeModificarAsync(CancellationToken ct = default)
        {
            // Sin validadores definidos - siempre permitido
            return Task.FromResult(ApiMotos.Application.Common.Validation.BusinessValidationResult.Success());
        }

        /// <summary>
        /// Valida si se puede eliminar esta instancia de Envio
        /// </summary>
        public Task<ApiMotos.Application.Common.Validation.BusinessValidationResult> PuedeEliminarAsync(CancellationToken ct = default)
        {
            // Sin validadores definidos - siempre permitido
            return Task.FromResult(ApiMotos.Application.Common.Validation.BusinessValidationResult.Success());
        }


    }
}
