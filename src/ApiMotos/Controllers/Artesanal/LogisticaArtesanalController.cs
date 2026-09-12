using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiMotos.Application.Artesanal.Logistica.SeguimientoEnvio;

namespace ApiMotos.Controllers.Artesanal
{
    /// <summary>
    /// Endpoints artesanales de logística (escena "Seguimiento del envío"). Solo GET, read-only
    /// (ver README). Las transiciones del ciclo siguen yendo por <c>POST api/Envio/{id}/{accion}</c>.
    /// La versión pública (sin login, para el QR) vive en SeguimientoPublicoController.
    /// </summary>
    [ApiController]
    [Route("api/artesanal")]
    [Authorize]
    public class LogisticaArtesanalController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LogisticaArtesanalController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Seguimiento interno del envío: hitos con SLA, cliente (con teléfono), pedido origen
        /// con precios y bitácora con usuario.
        /// </summary>
        [HttpGet("envio/{id:int}/seguimiento")]
        public async Task<IActionResult> Seguimiento([FromRoute] int id)
        {
            SeguimientoEnvioDto? result;
            try
            {
                result = await _mediator.Send(new SeguimientoEnvioQuery(id, null, Publico: false));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            if (result == null)
                return NotFound($"Envío con ID {id} no encontrado");
            return Ok(result);
        }
    }
}
