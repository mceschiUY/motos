using System.Globalization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiMotos.Application.Artesanal.Comercial.Agenda;
using ApiMotos.Application.Artesanal.Comercial.AvanceVendedor;
using ApiMotos.Application.Artesanal.Comercial.ClientesSinVisitar;
using ApiMotos.Application.Artesanal.Comercial.Comisiones;

namespace ApiMotos.Controllers.Artesanal
{
    /// <summary>
    /// Endpoints artesanales del frente comercial (Etapa A): agenda del vendedor, avance
    /// contra la meta y alerta de clientes sin visitar. Solo GET, read-only (ver README).
    /// </summary>
    [ApiController]
    [Route("api/artesanal")]
    [Authorize]
    public class ComercialArtesanalController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ComercialArtesanalController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Agenda: actividades planificadas (ProximaAccion en el rango) y realizadas (Fecha en el
        /// rango), lista plana ordenada por fecha. Fechas `yyyy-MM-dd`; default desde=hoy, hasta=hoy+7.
        /// </summary>
        [HttpGet("agenda")]
        public async Task<IActionResult> Agenda([FromQuery] int? vendedorId = null, [FromQuery] string? desde = null, [FromQuery] string? hasta = null)
        {
            if (!TryFecha(desde, out var dDesde)) return BadRequest("desde inválido: formato yyyy-MM-dd");
            if (!TryFecha(hasta, out var dHasta)) return BadRequest("hasta inválido: formato yyyy-MM-dd");
            var result = await _mediator.Send(new AgendaQuery(vendedorId, dDesde, dHasta));
            return Ok(result);
        }

        /// <summary>KPIs del vendedor en el período `YYYY-MM` (default: mes actual). 404 si el vendedor no existe.</summary>
        [HttpGet("vendedor/{id:int}/avance")]
        public async Task<IActionResult> Avance([FromRoute] int id, [FromQuery] string? periodo = null)
        {
            AvanceVendedorDto? result;
            try
            {
                result = await _mediator.Send(new AvanceVendedorQuery(id, periodo));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            if (result == null)
                return NotFound($"Vendedor con ID {id} no encontrado");
            return Ok(result);
        }

        /// <summary>
        /// Liquidación de comisiones del período `YYYY-MM` (default: mes actual): una fila por
        /// vendedor con pedidos entregados, total y comisión en USD. Etapa B, plan §4.4.
        /// </summary>
        [HttpGet("comisiones")]
        public async Task<IActionResult> Comisiones([FromQuery] string? periodo = null, [FromQuery] int? vendedorId = null)
        {
            try
            {
                var result = await _mediator.Send(new ComisionesQuery(periodo, vendedorId));
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>Clientes con vendedor asignado y sin actividad en los últimos `dias` (0 = el parámetro `crm.dias_sin_visita` de Configuración, default 30).</summary>
        [HttpGet("alertas/clientes-sin-visitar")]
        public async Task<IActionResult> ClientesSinVisitar([FromQuery] int dias = 0)
        {
            var result = await _mediator.Send(new ClientesSinVisitarQuery(dias));
            return Ok(result);
        }

        private static bool TryFecha(string? texto, out DateTime? fecha)
        {
            fecha = null;
            if (string.IsNullOrWhiteSpace(texto)) return true;
            if (DateTime.TryParseExact(texto.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var f))
            {
                fecha = f;
                return true;
            }
            return false;
        }
    }
}
