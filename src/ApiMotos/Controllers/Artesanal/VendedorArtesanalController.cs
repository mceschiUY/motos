using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiMotos.Application.Artesanal.Comercial.PanelVendedor;

namespace ApiMotos.Controllers.Artesanal
{
    /// <summary>
    /// Escena "panel del vendedor" (/vendedor/:id). Solo GET, read-only (ver README).
    /// El avance contra la meta, la agenda y las comisiones siguen en ComercialArtesanalController.
    /// </summary>
    [ApiController]
    [Route("api/artesanal")]
    [Authorize]
    public class VendedorArtesanalController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VendedorArtesanalController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Panel del vendedor: cabecera, cartera con semáforo, pedidos abiertos, comisión
        /// proyectada, ventas de las últimas 8 semanas y ranking del mes. 404 si no existe.
        /// </summary>
        [HttpGet("vendedor/{id:int}/panel")]
        public async Task<IActionResult> Panel([FromRoute] int id)
        {
            PanelVendedorDto? result;
            try
            {
                result = await _mediator.Send(new PanelVendedorQuery(id));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            if (result == null)
                return NotFound($"Vendedor con ID {id} no encontrado");
            return Ok(result);
        }
    }
}
