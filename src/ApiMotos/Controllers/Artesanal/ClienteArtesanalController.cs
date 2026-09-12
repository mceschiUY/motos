using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiMotos.Application.Artesanal.Comercial.Cliente360;

namespace ApiMotos.Controllers.Artesanal
{
    /// <summary>
    /// Endpoints artesanales de la escena "Cliente 360": la vista que abre el vendedor antes
    /// de visitar la tienda. Solo GET, read-only (ver README). Va aparte de
    /// ComercialArtesanalController para no pisar ese archivo.
    /// </summary>
    [ApiController]
    [Route("api/artesanal")]
    [Authorize]
    public class ClienteArtesanalController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClienteArtesanalController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Cliente 360: cabecera, salud de la relación (semáforo), línea de tiempo unificada
        /// (actividades + pedidos + envíos) y top 5 productos. 404 si el cliente no existe.
        /// </summary>
        [HttpGet("cliente/{id:int}/360")]
        public async Task<IActionResult> Cliente360([FromRoute] int id)
        {
            Cliente360Dto? result;
            try
            {
                result = await _mediator.Send(new Cliente360Query(id));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            if (result == null)
                return NotFound($"Cliente con ID {id} no encontrado");
            return Ok(result);
        }
    }
}
