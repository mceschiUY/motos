using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiMotos.Application.Agregates.Categorias.Commands.Crear;
using ApiMotos.Application.Agregates.Categorias.Commands.Modificar;
using ApiMotos.Application.Agregates.Categorias.Commands.Eliminar;
using ApiMotos.Application.Agregates.Categorias.Queries.Categorias;
using ApiMotos.Application.Agregates.Categorias.Queries.Resumen;
using ApiMotos.Application.Agregates.Categorias.Queries.Buscar;

namespace ApiMotos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? skip = null, [FromQuery] int? take = null)
        {
            var result = await _mediator.Send(new CategoriasQuery(Skip: skip, Take: take));
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var result = await _mediator.Send(new CategoriasQuery(Id: id));
            var item = result.FirstOrDefault();
            if (item == null)
                return NotFound($"Categoria con ID {id} no encontrado");
            return Ok(item);
        }

        [HttpGet("resumen")]
        public async Task<IActionResult> Resumen()
        {
            var result = await _mediator.Send(new CategoriasResumenQuery());
            return Ok(result);
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string q)
        {
            var result = await _mediator.Send(new CategoriasBuscarQuery(q ?? ""));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearCategoriaCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(new { id = result.Value });
            return Rechazo(result.Errors.Select(e => e.Message));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ModificarCategoriaCommand command)
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
            var result = await _mediator.Send(new EliminarCategoriaCommand(id));
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
