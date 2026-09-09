using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiMotos.Application.Agregates.Actividades.Commands.Crear;
using ApiMotos.Application.Agregates.Actividades.Commands.Modificar;
using ApiMotos.Application.Agregates.Actividades.Commands.Eliminar;
using ApiMotos.Application.Agregates.Actividades.Queries.Actividades;
using ApiMotos.Application.Agregates.Actividades.Queries.Resumen;
using ApiMotos.Application.Agregates.Actividades.Queries.Buscar;
using ApiMotos.Application.Agregates.Actividades.Queries.ByVendedorId;
using ApiMotos.Application.Agregates.Actividades.Queries.ByClienteId;

namespace ApiMotos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ActividadController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ActividadController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? skip = null, [FromQuery] int? take = null)
        {
            var result = await _mediator.Send(new ActividadesQuery(Skip: skip, Take: take));
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var result = await _mediator.Send(new ActividadesQuery(Id: id));
            var item = result.FirstOrDefault();
            if (item == null)
                return NotFound($"Actividad con ID {id} no encontrada");
            return Ok(item);
        }

        /// <summary>Agregados para el dashboard: total, por resultado, por mes (GROUP BY en SQL).</summary>
        [HttpGet("resumen")]
        public async Task<IActionResult> Resumen()
        {
            var result = await _mediator.Send(new ActividadesResumenQuery());
            return Ok(result);
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string q)
        {
            var result = await _mediator.Send(new ActividadesBuscarQuery(q ?? ""));
            return Ok(result);
        }

        [HttpGet("by-vendedor/{id}")]
        public async Task<IActionResult> GetByVendedorId([FromRoute] int id)
        {
            var result = await _mediator.Send(new ActividadesByVendedorIdQuery { VendedorId = id });
            return Ok(result);
        }

        [HttpGet("by-cliente/{id}")]
        public async Task<IActionResult> GetByClienteId([FromRoute] int id)
        {
            var result = await _mediator.Send(new ActividadesByClienteIdQuery { ClienteId = id });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearActividadCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(new { id = result.Value });
            return Rechazo(result.Errors.Select(e => e.Message));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ModificarActividadCommand command)
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
            var result = await _mediator.Send(new EliminarActividadCommand(id));
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
