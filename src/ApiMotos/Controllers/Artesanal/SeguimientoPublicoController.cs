using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiMotos.Application.Artesanal.Logistica.SeguimientoEnvio;

namespace ApiMotos.Controllers.Artesanal
{
    /// <summary>
    /// Seguimiento PÚBLICO del envío: lo abre el cliente de la tienda desde el QR o el link
    /// compartido, sin login. Único endpoint anónimo fuera de auth/configuracion.
    ///
    /// Acceso anónimo: [AllowAnonymous] a nivel controller (no hay FallbackPolicy que exija
    /// usuario) y la query lleva [ZasAllowAnonymous] para que el SecurityBehavior de MediatR
    /// no la rechace. Lo que NO viaja lo recorta la query con Publico=true: precios, costos,
    /// totales, teléfonos y usuarios. Solo GET.
    /// </summary>
    [ApiController]
    [Route("api/publico")]
    [AllowAnonymous]
    public class SeguimientoPublicoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SeguimientoPublicoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Seguimiento por código de rastreo (sin distinguir mayúsculas). 404 si no existe.</summary>
        [HttpGet("seguimiento/{codigo}")]
        public async Task<IActionResult> Seguimiento([FromRoute] string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return NotFound("No encontramos un envío con ese código");

            var result = await _mediator.Send(new SeguimientoEnvioQuery(null, codigo, Publico: true));
            if (result == null)
                return NotFound("No encontramos un envío con ese código");
            return Ok(result);
        }
    }
}
