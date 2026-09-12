using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiMotos.Application.Artesanal.Control.CentroControl;

namespace ApiMotos.Controllers.Artesanal
{
    /// <summary>
    /// Endpoints artesanales del centro de control (home "Hoy" y modo pantalla).
    /// Solo GET, read-only (ver README de modules/artesanal).
    /// </summary>
    [ApiController]
    [Route("api/artesanal")]
    [Authorize]
    public class ControlArtesanalController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ControlArtesanalController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// HOY en una sola llamada: KPI del día, feed de acciones ordenado por urgencia,
        /// pipeline de pedidos del mes, equipo contra su meta, top productos y stock por depósito.
        /// </summary>
        [HttpGet("centro-control")]
        public async Task<IActionResult> CentroControl()
        {
            var result = await _mediator.Send(new CentroControlQuery());
            return Ok(result);
        }
    }
}
