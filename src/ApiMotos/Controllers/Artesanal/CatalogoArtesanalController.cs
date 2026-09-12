using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiMotos.Application.Artesanal.Catalogo.Catalogo;
using ApiMotos.Application.Artesanal.Catalogo.FichaProducto;
using ApiMotos.Application.Artesanal.Catalogo.PreciosVariante;

namespace ApiMotos.Controllers.Artesanal
{
    /// <summary>
    /// Endpoints artesanales del catálogo premium (Etapa C, plan §4.5): grilla de cards, ficha
    /// comercial con la matriz talla × color e historial de precios de un SKU. Solo GET,
    /// read-only (ver README). Va aparte de ComercialArtesanalController: ese es fuerza de ventas.
    /// </summary>
    [ApiController]
    [Route("api/artesanal")]
    [Authorize]
    public class CatalogoArtesanalController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CatalogoArtesanalController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Catálogo navegable: productos activos con precio "desde", stock y portada. Destacados primero.</summary>
        [HttpGet("catalogo")]
        public async Task<IActionResult> Catalogo([FromQuery] int? marcaId = null, [FromQuery] int? categoriaId = null, [FromQuery] string? q = null)
        {
            var result = await _mediator.Send(new CatalogoQuery(marcaId, categoriaId, q));
            return Ok(result);
        }

        /// <summary>
        /// Ficha comercial: cabecera del producto y matriz de variantes con existencias, precio y
        /// margen. `depositoId` opcional; sin él, las existencias suman todos los depósitos.
        /// </summary>
        [HttpGet("catalogo/{id:int}")]
        public async Task<IActionResult> Ficha([FromRoute] int id, [FromQuery] int? depositoId = null)
        {
            FichaProductoDto? result;
            try
            {
                result = await _mediator.Send(new FichaProductoQuery(id, depositoId));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            if (result == null)
                return NotFound($"Producto con ID {id} no encontrado");
            return Ok(result);
        }

        /// <summary>Historial de precio y costo de una variante, del cambio más nuevo al más viejo.</summary>
        [HttpGet("variante/{id:int}/precios")]
        public async Task<IActionResult> Precios([FromRoute] int id)
        {
            var result = await _mediator.Send(new PreciosVarianteQuery(id));
            return Ok(result);
        }
    }
}
