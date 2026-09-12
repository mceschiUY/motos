using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiMotos.Application.Artesanal.Catalogo.FichaSku;
using ApiMotos.Application.Artesanal.Catalogo.Existencias;

namespace ApiMotos.Controllers.Artesanal
{
    /// <summary>
    /// Escena "Ficha de SKU": la variante contada completa (stock por depósito, Kardex con saldo
    /// corrido, comprometido, cobertura y pedidos abiertos). Solo GET, read-only (ver README).
    /// El historial de precios sigue en CatalogoArtesanalController (variante/{id}/precios).
    /// </summary>
    [ApiController]
    [Route("api/artesanal")]
    [Authorize]
    public class SkuArtesanalController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SkuArtesanalController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Ficha completa de un SKU. 404 si la variante no existe.</summary>
        [HttpGet("sku/{id:int}")]
        public async Task<IActionResult> Ficha([FromRoute] int id)
        {
            FichaSkuDto? result;
            try
            {
                result = await _mediator.Send(new FichaSkuQuery(id));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            if (result == null)
                return NotFound($"Variante con ID {id} no encontrada");
            return Ok(result);
        }

        /// <summary>
        /// Existencias agrupables (escena "¿qué se está acabando?"): una fila por SKU con
        /// categoría raíz, saldo por depósito, comprometido, ventas de 30 días, cobertura y
        /// semáforo. `depositoId` opcional; sin él, el saldo suma todos los depósitos.
        /// </summary>
        [HttpGet("existencias")]
        public async Task<IActionResult> Existencias([FromQuery] int? depositoId = null)
        {
            var result = await _mediator.Send(new ExistenciasQuery(depositoId));
            return Ok(result);
        }
    }
}
