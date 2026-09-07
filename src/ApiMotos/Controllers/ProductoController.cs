using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiMotos.Application.Agregates.Productos.Commands.Crear;
using ApiMotos.Application.Agregates.Productos.Commands.Modificar;
using ApiMotos.Application.Agregates.Productos.Commands.Eliminar;
using ApiMotos.Application.Agregates.Productos.Queries.Productos;
using ApiMotos.Application.Agregates.Productos.Queries.Resumen;
using ApiMotos.Application.Agregates.Productos.Queries.Buscar;
using ApiMotos.Application.Agregates.Productos.Queries.ByMarcaId;
using ApiMotos.Application.Agregates.Productos.Queries.ByCategoriaId;

namespace ApiMotos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? skip = null, [FromQuery] int? take = null)
        {
            var result = await _mediator.Send(new ProductosQuery(Skip: skip, Take: take));
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var result = await _mediator.Send(new ProductosQuery(Id: id));
            var item = result.FirstOrDefault();
            if (item == null)
                return NotFound($"Producto con ID {id} no encontrado");
            return Ok(item);
        }

        [HttpGet("resumen")]
        public async Task<IActionResult> Resumen()
        {
            var result = await _mediator.Send(new ProductosResumenQuery());
            return Ok(result);
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string q)
        {
            var result = await _mediator.Send(new ProductosBuscarQuery(q ?? ""));
            return Ok(result);
        }

        [HttpGet("by-marca/{id}")]
        public async Task<IActionResult> GetByMarcaId([FromRoute] int id)
        {
            var result = await _mediator.Send(new ProductosByMarcaIdQuery { MarcaId = id });
            return Ok(result);
        }

        [HttpGet("by-categoria/{id}")]
        public async Task<IActionResult> GetByCategoriaId([FromRoute] int id)
        {
            var result = await _mediator.Send(new ProductosByCategoriaIdQuery { CategoriaId = id });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearProductoCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(new { id = result.Value });
            return Rechazo(result.Errors.Select(e => e.Message));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ModificarProductoCommand command)
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
            var result = await _mediator.Send(new EliminarProductoCommand(id));
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
