using FluentResults;
using NSpecifications;
using ApiMotos.Domain.Agregates.Pedidos;
using ApiMotos.Domain.Agregates.PedidoLineas;
using ApiMotos.Domain.Agregates.MovimientosStock;
using ApiMotos.Domain.Agregates.Envios;
using ApiMotos.Domain.Agregates.Vendedores;
using ApiMotos.Domain.Common;
using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Pedidos.Commands.Crear;
using ApiMotos.Application.Agregates.Pedidos.Commands.Modificar;

namespace ApiMotos.Application.Agregates.Pedidos
{
    /// <summary>
    /// Reglas de negocio del Pedido (plan §3, Etapa B). Acá vive TODO lo que el ciclo
    /// desencadena — la matriz de transiciones es dato (ciclos-vida.json), los efectos son esto:
    ///
    ///   crear      → sella el número PED-000n (derivado del Id: único sin carrera).
    ///   confirmar  → exige al menos una línea; avisa si falta stock, pero deja pasar
    ///                (backorder simple, plan §7: el aviso queda en Observaciones).
    ///   despachar  → una SALIDA de Kardex por línea (DocumentoOrigen = Numero) y nace el
    ///                Envío con la agencia elegida; el pedido guarda su EnvioId.
    ///   entregar   → fija la comisión = Total × % del vendedor.
    ///   anular     → si ya había salido del depósito, genera la ENTRADA de reversa.
    ///
    /// Los efectos van en DespuesDeAccion (después de persistir el estado) y son
    /// IDEMPOTENTES: se miran contra el Kardex, así que repetir la acción no duplica nada.
    /// </summary>
    public class PedidoHooks : CrudHooks<Pedido, CrearPedidoCommand, ModificarPedidoCommand>
    {
        private readonly IPedidoRepositorio _pedidos;
        private readonly IPedidoLineaRepositorio _lineas;
        private readonly IMovimientoStockRepositorio _movimientos;
        private readonly IEnvioRepositorio _envios;
        private readonly IVendedorRepositorio _vendedores;
        private readonly IQueryService _consultas;

        public PedidoHooks(IPedidoRepositorio pedidos, IPedidoLineaRepositorio lineas,
            IMovimientoStockRepositorio movimientos, IEnvioRepositorio envios,
            IVendedorRepositorio vendedores, IQueryService consultas, IReglasNegocioEjecutor motor)
            : base(motor, "Pedido")
        {
            _pedidos = pedidos;
            _lineas = lineas;
            _movimientos = movimientos;
            _envios = envios;
            _vendedores = vendedores;
            _consultas = consultas;
        }

        // ─── Alta: número del pedido ────────────────────────────────────────────

        public override Task<Result> DespuesDeCrear(CrearPedidoCommand comando, Pedido creada, CancellationToken ct)
        {
            // PED-000n derivado del Id: único por construcción, sin consultar el máximo.
            creada.SellarNumero($"PED-{creada.Id:D4}");
            _pedidos.Update(creada);
            return Task.FromResult(Result.Ok());
        }

        // ─── Borrado: solo un borrador sin rastro operativo ─────────────────────

        public override async Task<Result> AntesDeEliminar(Pedido entidad, CancellationToken ct)
        {
            if (entidad.Estado != "borrador")
                return Result.Fail($"Solo se puede eliminar un pedido en borrador (este está {entidad.Estado}). Anulalo en su lugar.");
            var lineas = await _lineas.GetPedidoLineasAsync(new Spec<PedidoLinea>(l => l.PedidoId == entidad.Id));
            if (lineas.Count > 0)
                return Result.Fail("El pedido tiene líneas cargadas: borralas primero");
            return Result.Ok();
        }

        // ─── Guardas de las transiciones ────────────────────────────────────────

        public override async Task<Result> AntesDeAccion(string accion, Pedido entidad, CancellationToken ct)
        {
            switch (accion)
            {
                case "PasarAConfirmado":
                    var lineas = await _lineas.GetPedidoLineasAsync(new Spec<PedidoLinea>(l => l.PedidoId == entidad.Id));
                    if (lineas.Count == 0)
                        return Result.Fail("Un pedido sin líneas no se puede confirmar");
                    return Result.Ok();

                case "PasarADespachado":
                    if (entidad.AgenciaId is null or <= 0)
                        return Result.Fail("Elegí la agencia antes de despachar: el envío nace con ella");
                    return Result.Ok();

                default:
                    return Result.Ok();
            }
        }

        // ─── Efectos de las transiciones ────────────────────────────────────────

        public override async Task<Result> DespuesDeAccion(string accion, Pedido entidad, CancellationToken ct)
        {
            switch (accion)
            {
                case "PasarAConfirmado": return await AvisarFaltantes(entidad);
                case "PasarADespachado": return await Despachar(entidad);
                case "PasarAEntregado": return await FijarComision(entidad);
                case "PasarAAnulado": return await RevertirSiHizoFalta(entidad);
                default: return Result.Ok();
            }
        }

        /// <summary>
        /// Backorder simple (plan §7): confirmar NO exige stock, pero deja dicho lo que falta.
        /// El aviso se escribe una sola vez y no pisa lo que escribió el vendedor.
        /// </summary>
        private async Task<Result> AvisarFaltantes(Pedido pedido)
        {
            var faltantes = await _consultas.ConsultarAsync<FaltanteFila>(SqlFaltantes,
                new { PedidoId = pedido.Id, DepositoId = pedido.DepositoId });
            if (faltantes.Count == 0) return Result.Ok();

            var detalle = string.Join(", ", faltantes.Take(5).Select(f => $"{f.Sku} (pide {f.Pedido:0.##}, hay {f.Disponible:0.##})"));
            var aviso = $"Sin stock suficiente al confirmar: {detalle}";
            if (pedido.Observaciones?.Contains("Sin stock suficiente al confirmar") != true)
            {
                pedido.AgregarObservacion(aviso);
                _pedidos.Update(pedido);
            }
            return Result.Ok();
        }

        /// <summary>
        /// Despachar = el stock sale del depósito y nace el envío. El Kardex es la verdad:
        /// si ya hay salidas de este pedido, no vuelve a escribirlas (idempotente).
        /// </summary>
        private async Task<Result> Despachar(Pedido pedido)
        {
            var lineas = await _lineas.GetPedidoLineasAsync(new Spec<PedidoLinea>(l => l.PedidoId == pedido.Id));
            if (lineas.Count == 0)
                return Result.Fail("No se puede despachar un pedido sin líneas");

            var yaSalio = await _movimientos.GetMovimientosStockAsync(
                new Spec<MovimientoStock>(m => m.DocumentoOrigen == pedido.Numero && m.Tipo == "salida"));
            if (yaSalio.Count == 0)
            {
                foreach (var linea in lineas)
                {
                    var movimiento = MovimientoStock.Crear(linea.VarianteId, pedido.DepositoId, null, "salida",
                        linea.Cantidad, null, $"Despacho del pedido {pedido.Numero}", pedido.Numero,
                        Clock.Current.Now, null);
                    if (movimiento.IsFailed) return Result.Fail(movimiento.Errors);
                    await _movimientos.AddAsync(movimiento.Value);
                }
            }

            if (pedido.EnvioId is null or <= 0 && pedido.AgenciaId is > 0)
            {
                var ahora = Clock.Current.Now;
                // Las fechas de las etapas no alcanzadas repiten FechaRecibido (contrato del Envío).
                var envio = Envio.Crear(pedido.Numero, "recibido", ahora, ahora, ahora, ahora, string.Empty,
                    pedido.ClienteId, pedido.AgenciaId!.Value, pedido.Id);
                if (envio.IsFailed) return Result.Fail(envio.Errors);
                var creado = await _envios.AddAsync(envio.Value);
                pedido.AsignarEnvio(creado.Id);
                _pedidos.Update(pedido);
            }

            return Result.Ok();
        }

        /// <summary>Comisión = Total × % del vendedor, sellada al entregar (plan §3.4).</summary>
        private async Task<Result> FijarComision(Pedido pedido)
        {
            var vendedor = await _vendedores.FindAsync(pedido.VendedorId);
            var porcentaje = vendedor?.ComisionPorcentaje ?? 0m;
            pedido.FijarComision(Math.Round(pedido.TotalUsd * porcentaje / 100m, 2, MidpointRounding.AwayFromZero));
            _pedidos.Update(pedido);
            return Result.Ok();
        }

        /// <summary>
        /// Anular un pedido que ya había salido del depósito devuelve la mercadería al Kardex
        /// (entrada de reversa). El Kardex nunca se edita ni se borra: se compensa.
        /// </summary>
        private async Task<Result> RevertirSiHizoFalta(Pedido pedido)
        {
            var delPedido = await _movimientos.GetMovimientosStockAsync(
                new Spec<MovimientoStock>(m => m.DocumentoOrigen == pedido.Numero));
            var salidas = delPedido.Where(m => m.Tipo == "salida").ToList();
            if (salidas.Count == 0) return Result.Ok();
            if (delPedido.Any(m => m.Tipo == "entrada")) return Result.Ok();   // ya se revirtió

            foreach (var salida in salidas)
            {
                var reversa = MovimientoStock.Crear(salida.VarianteId, salida.DepositoId, null, "entrada",
                    salida.Cantidad, salida.CostoUnitario, $"Reversa por anulación del pedido {pedido.Numero}",
                    pedido.Numero, Clock.Current.Now, null);
                if (reversa.IsFailed) return Result.Fail(reversa.Errors);
                await _movimientos.AddAsync(reversa.Value);
            }
            return Result.Ok();
        }

        private sealed class FaltanteFila
        {
            public string? Sku { get; set; }
            public decimal Pedido { get; set; }
            public decimal Disponible { get; set; }
        }

        /// <summary>Lo pedido por SKU contra el saldo del Kardex en el depósito del pedido
        /// (mismo cálculo que ExistenciasQuery, acotado a las variantes de este pedido).</summary>
        private const string SqlFaltantes = @"
WITH pedido AS (
    SELECT l.VarianteId, SUM(l.Cantidad) AS Cantidad
    FROM PC_PEDIDO_LINEAS l
    WHERE l.PedidoId = @PedidoId
    GROUP BY l.VarianteId
), mov AS (
    SELECT VarianteId, DepositoId,
        CASE Tipo WHEN 'entrada' THEN Cantidad
                  WHEN 'salida' THEN -Cantidad
                  WHEN 'ajuste' THEN Cantidad
                  WHEN 'transferencia' THEN -Cantidad END AS Delta
    FROM PC_MOVIMIENTOS_STOCK
    UNION ALL
    SELECT VarianteId, DepositoDestinoId AS DepositoId, Cantidad AS Delta
    FROM PC_MOVIMIENTOS_STOCK
    WHERE Tipo = 'transferencia' AND DepositoDestinoId IS NOT NULL
), saldo AS (
    SELECT VarianteId, SUM(Delta) AS Disponible
    FROM mov WHERE DepositoId = @DepositoId
    GROUP BY VarianteId
)
SELECT v.Sku AS Sku, pedido.Cantidad AS Pedido, ISNULL(saldo.Disponible, 0) AS Disponible
FROM pedido
LEFT JOIN saldo ON saldo.VarianteId = pedido.VarianteId
LEFT JOIN PC_VARIANTES v ON v.Id = pedido.VarianteId
WHERE ISNULL(saldo.Disponible, 0) < pedido.Cantidad
ORDER BY v.Sku";
    }
}
