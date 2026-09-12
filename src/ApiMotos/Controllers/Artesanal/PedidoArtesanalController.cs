using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiMotos.Application.Artesanal.Comercial.FichaPedido;

namespace ApiMotos.Controllers.Artesanal
{
    /// <summary>
    /// Escena "Pedido" (revisión de escenas 2026-09-12): la ficha del pedido contada completa.
    /// Solo GET, read-only (ver README); las transiciones del ciclo siguen en
    /// <c>POST api/Pedido/{id}/{accion}</c> y las líneas en <c>api/PedidoLinea</c>.
    /// </summary>
    [ApiController]
    [Route("api/artesanal")]
    [Authorize]
    public class PedidoArtesanalController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PedidoArtesanalController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Cabecera, líneas con foto/talle/color/stock, totales y envío del pedido. 404 si no existe.</summary>
        [HttpGet("pedido/{id:int}/ficha")]
        public async Task<IActionResult> Ficha([FromRoute] int id)
        {
            FichaPedidoDto? result;
            try
            {
                result = await _mediator.Send(new FichaPedidoQuery(id));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            if (result == null)
                return NotFound($"Pedido con ID {id} no encontrado");
            return Ok(result);
        }
    }
}
