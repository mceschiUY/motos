using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiMotos.Application.Agregates.Marcas.Commands.Crear;
using ApiMotos.Application.Agregates.Marcas.Commands.Modificar;
using ApiMotos.Application.Agregates.Marcas.Commands.Eliminar;
using ApiMotos.Application.Agregates.Marcas.Queries.Marcas;
using ApiMotos.Application.Agregates.Marcas.Queries.Resumen;
using ApiMotos.Application.Agregates.Marcas.Queries.Buscar;

namespace ApiMotos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MarcaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MarcaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? skip = null, [FromQuery] int? take = null)
        {
            var result = await _mediator.Send(new MarcasQuery(Skip: skip, Take: take));
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var result = await _mediator.Send(new MarcasQuery(Id: id));
            var item = result.FirstOrDefault();
            if (item == null)
                return NotFound($"Marca con ID {id} no encontrado");
            return Ok(item);
        }

        [HttpGet("resumen")]
        public async Task<IActionResult> Resumen()
        {
            var result = await _mediator.Send(new MarcasResumenQuery());
            return Ok(result);
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string q)
        {
            var result = await _mediator.Send(new MarcasBuscarQuery(q ?? ""));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearMarcaCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(new { id = result.Value });
            return Rechazo(result.Errors.Select(e => e.Message));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ModificarMarcaCommand command)
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
            var result = await _mediator.Send(new EliminarMarcaCommand(id));
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
