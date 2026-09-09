using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiMotos.Application.Agregates.Pedidos.Commands.Crear;
using ApiMotos.Application.Agregates.Pedidos.Commands.Modificar;
using ApiMotos.Application.Agregates.Pedidos.Commands.Eliminar;
using ApiMotos.Application.Agregates.Pedidos.Commands.Transicion;
using ApiMotos.Application.Agregates.Pedidos.Queries.Pedidos;
using ApiMotos.Application.Agregates.Pedidos.Queries.Resumen;
using ApiMotos.Application.Agregates.Pedidos.Queries.Buscar;
using ApiMotos.Application.Agregates.Pedidos.Queries.ByClienteId;
using ApiMotos.Application.Agregates.Pedidos.Queries.ByVendedorId;

namespace ApiMotos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PedidoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PedidoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? skip = null, [FromQuery] int? take = null)
        {
            var result = await _mediator.Send(new PedidosQuery(Skip: skip, Take: take));
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var result = await _mediator.Send(new PedidosQuery(Id: id));
            var item = result.FirstOrDefault();
            if (item == null)
                return NotFound($"Pedido con ID {id} no encontrado");
            return Ok(item);
        }

        /// <summary>Agregados para el dashboard: total, por estado, por mes (GROUP BY en SQL).</summary>
        [HttpGet("resumen")]
        public async Task<IActionResult> Resumen()
        {
            var result = await _mediator.Send(new PedidosResumenQuery());
            return Ok(result);
        }

        /// <summary>Búsqueda de texto (omnibox global): LIKE en número, cliente y estado.</summary>
        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string q)
        {
            var result = await _mediator.Send(new PedidosBuscarQuery(q ?? ""));
            return Ok(result);
        }

        [HttpGet("by-cliente/{id:int}")]
        public async Task<IActionResult> GetByClienteId([FromRoute] int id)
        {
            var result = await _mediator.Send(new PedidosByClienteIdQuery { ClienteId = id });
            return Ok(result);
        }

        [HttpGet("by-vendedor/{id:int}")]
        public async Task<IActionResult> GetByVendedorId([FromRoute] int id)
        {
            var result = await _mediator.Send(new PedidosByVendedorIdQuery { VendedorId = id });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearPedidoCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(new { id = result.Value });
            return Rechazo(result.Errors.Select(e => e.Message));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ModificarPedidoCommand command)
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
            var result = await _mediator.Send(new EliminarPedidoCommand(id));
            if (result.IsSuccess)
                return Ok(result.Value);
            return Rechazo(result.Errors.Select(e => e.Message));
        }

        /// <summary>
        /// Transiciones del ciclo (pasar-a-confirmado, pasar-a-preparado, pasar-a-despachado,
        /// pasar-a-entregado, pasar-a-anulado) — la matriz vive en ciclos-vida.json y los
        /// efectos (Kardex, Envío, comisión) en PedidoHooks.
        /// </summary>
        [HttpPost("{id:int}/{accion}")]
        public async Task<IActionResult> Transicion([FromRoute] int id, [FromRoute] string accion)
        {
            var result = await _mediator.Send(new TransicionPedidoCommand(id, accion));
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
