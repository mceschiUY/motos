using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiMotos.Application.Agregates.MovimientosStock.Commands.Crear;
using ApiMotos.Application.Agregates.MovimientosStock.Commands.Modificar;
using ApiMotos.Application.Agregates.MovimientosStock.Commands.Eliminar;
using ApiMotos.Application.Agregates.MovimientosStock.Queries.MovimientosStock;
using ApiMotos.Application.Agregates.MovimientosStock.Queries.Resumen;
using ApiMotos.Application.Agregates.MovimientosStock.Queries.Buscar;
using ApiMotos.Application.Agregates.MovimientosStock.Queries.ByVarianteId;
using ApiMotos.Application.Agregates.MovimientosStock.Queries.ByDepositoId;
using ApiMotos.Application.Agregates.MovimientosStock.Queries.Existencias;

namespace ApiMotos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MovimientoStockController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MovimientoStockController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? skip = null, [FromQuery] int? take = null)
        {
            var result = await _mediator.Send(new MovimientosStockQuery(Skip: skip, Take: take));
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var result = await _mediator.Send(new MovimientosStockQuery(Id: id));
            var item = result.FirstOrDefault();
            if (item == null)
                return NotFound($"MovimientoStock con ID {id} no encontrado");
            return Ok(item);
        }

        [HttpGet("resumen")]
        public async Task<IActionResult> Resumen()
        {
            var result = await _mediator.Send(new MovimientosStockResumenQuery());
            return Ok(result);
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string q)
        {
            var result = await _mediator.Send(new MovimientosStockBuscarQuery(q ?? ""));
            return Ok(result);
        }

        [HttpGet("by-variante/{id}")]
        public async Task<IActionResult> GetByVarianteId([FromRoute] int id)
        {
            var result = await _mediator.Send(new MovimientosStockByVarianteIdQuery { VarianteId = id });
            return Ok(result);
        }

        [HttpGet("by-deposito/{id}")]
        public async Task<IActionResult> GetByDepositoId([FromRoute] int id)
        {
            var result = await _mediator.Send(new MovimientosStockByDepositoIdQuery { DepositoId = id });
            return Ok(result);
        }

        /// <summary>Existencias (saldo disponible) por SKU/depósito, calculado sumando el Kardex.</summary>
        [HttpGet("existencias")]
        public async Task<IActionResult> Existencias([FromQuery] int? varianteId = null, [FromQuery] int? depositoId = null)
        {
            var result = await _mediator.Send(new ExistenciasQuery(VarianteId: varianteId, DepositoId: depositoId));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearMovimientoStockCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(new { id = result.Value });
            return Rechazo(result.Errors.Select(e => e.Message));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ModificarMovimientoStockCommand command)
        {
            if (id != command.Id)
                return BadRequest("El id de la ruta no coincide con el id del comando");
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(result.Value);
            return Rechazo(result.Errors.Select(e => e.Message));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var result = await _mediator.Send(new EliminarMovimientoStockCommand(id));
            if (result.IsSuccess)
                return Ok(result.Value);
            return Rechazo(result.Errors.Select(e => e.Message));
        }

        private IActionResult Rechazo(IEnumerable<string> mensajes)
        {
            var lista = mensajes.ToList();
            return new ObjectResult(new
            {
                type = "https://httpstatuses.io/400",
                title = "Business Rule Violation",
                status = 400,
                detail = string.Join("; ", lista),
                instance = Request.Path.Value ?? "",
                errors = lista.Select(m => new { message = m }).ToList(),
                traceId = HttpContext.TraceIdentifier,
            })
            { StatusCode = StatusCodes.Status400BadRequest, ContentTypes = { "application/problem+json" } };
        }
    }
}
