using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiMotos.Application.Agregates.PedidoLineas.Commands.Crear;
using ApiMotos.Application.Agregates.PedidoLineas.Commands.Modificar;
using ApiMotos.Application.Agregates.PedidoLineas.Commands.Eliminar;
using ApiMotos.Application.Agregates.PedidoLineas.Queries.PedidoLineas;
using ApiMotos.Application.Agregates.PedidoLineas.Queries.Resumen;
using ApiMotos.Application.Agregates.PedidoLineas.Queries.Buscar;
using ApiMotos.Application.Agregates.PedidoLineas.Queries.ByPedidoId;

namespace ApiMotos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PedidoLineaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PedidoLineaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? skip = null, [FromQuery] int? take = null)
        {
            var result = await _mediator.Send(new PedidoLineasQuery(Skip: skip, Take: take));
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var result = await _mediator.Send(new PedidoLineasQuery(Id: id));
            var item = result.FirstOrDefault();
            if (item == null)
                return NotFound($"PedidoLinea con ID {id} no encontrado");
            return Ok(item);
        }

        [HttpGet("resumen")]
        public async Task<IActionResult> Resumen()
        {
            var result = await _mediator.Send(new PedidoLineasResumenQuery());
            return Ok(result);
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string q)
        {
            var result = await _mediator.Send(new PedidoLineasBuscarQuery(q ?? ""));
            return Ok(result);
        }

        [HttpGet("by-pedido/{id:int}")]
        public async Task<IActionResult> GetByPedidoId([FromRoute] int id)
        {
            var result = await _mediator.Send(new PedidoLineasByPedidoIdQuery { PedidoId = id });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearPedidoLineaCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(new { id = result.Value });
            return Rechazo(result.Errors.Select(e => e.Message));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ModificarPedidoLineaCommand command)
        {
            if (id != command.Id)
                return BadRequest("El id de la ruta no coincide con el id del comando");
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(result.Value);
            return Rechazo(result.Errors.Select(e => e.Message));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var result = await _mediator.Send(new EliminarPedidoLineaCommand(id));
            if (result.IsSuccess)
                return Ok(result.Value);
            return Rechazo(result.Errors.Select(e => e.Message));
        }

        /// <summary>Rechazo de reglas de negocio como RFC 7807 problem+json (Ola 1).</summary>
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
