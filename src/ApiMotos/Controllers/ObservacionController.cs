using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiMotos.Application.Agregates.Observaciones.Commands.Crear;
using ApiMotos.Application.Agregates.Observaciones.Commands.Modificar;
using ApiMotos.Application.Agregates.Observaciones.Commands.Eliminar;
using ApiMotos.Application.Agregates.Observaciones.Queries.Observaciones;
using ApiMotos.Application.Agregates.Observaciones.Queries.Resumen;
using ApiMotos.Application.Agregates.Observaciones.Queries.Buscar;
using ApiMotos.Application.Agregates.Observaciones.Queries.ByEnvioId;

namespace ApiMotos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // R: toda la API de dominio exige usuario autenticado (F4 seguridad)
    public class ObservacionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ObservacionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? skip = null, [FromQuery] int? take = null)
        {
            var query = new ObservacionesQuery(Skip: skip, Take: take);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            // El filtro va en el SQL (O(1)), no en memoria
            var query = new ObservacionesQuery(Id: id);
            var result = await _mediator.Send(query);
            var item = result.FirstOrDefault();
            if (item == null)
                return NotFound($"Observacion con ID {id} no encontrado");
            return Ok(item);
        }

        /// <summary>Agregados para el dashboard: total, por estado, por mes (GROUP BY en SQL).</summary>
        [HttpGet("resumen")]
        public async Task<IActionResult> Resumen()
        {
            var result = await _mediator.Send(new ObservacionesResumenQuery());
            return Ok(result);
        }

        /// <summary>Búsqueda de texto (omnibox global): LIKE en los campos string, TOP 10.</summary>
        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string q)
        {
            var result = await _mediator.Send(new ObservacionesBuscarQuery(q ?? ""));
            return Ok(result);
        }
        [HttpGet("by-envio/{id}")]
        public async Task<IActionResult> GetByEnvioId([FromRoute] int id)
        {
            var query = new ObservacionesByEnvioIdQuery { EnvioId = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearObservacionCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                // Objeto { id } (no el int pelado): el form lo ignora, pero el runner de tests
                // captura el FK leyendo la propiedad 'id' del cuerpo para encadenar padre→hijo.
                return Ok(new { id = result.Value });
            // RFC 7807 problem+json (Ola 1): errors[].message conserva el contrato del form.
            return Rechazo(result.Errors.Select(e => e.Message));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ModificarObservacionCommand command)
        {
            if (id != command.Id)
                return BadRequest("El id de la ruta no coincide con el id del comando");
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(result.Value);
            // RFC 7807 problem+json (Ola 1): errors[].message conserva el contrato del form.
            return Rechazo(result.Errors.Select(e => e.Message));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var command = new EliminarObservacionCommand(id);
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(result.Value);
            // RFC 7807 problem+json (Ola 1): errors[].message conserva el contrato del form.
            return Rechazo(result.Errors.Select(e => e.Message));
        }

        /// <summary>Rechazo de reglas de negocio como RFC 7807 problem+json (Ola 1).
        /// errors[].message conserva el contrato del form; detail concentra el texto del
        /// rechazo (el runner de tests lo asevera con esperar_texto).</summary>
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

        // ═══════════════════════════════════════════════════════════════════════════════
        // SISTEMA DE VALIDACIÓN UNIVERSAL - Endpoints de pre-validación
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Pre-valida si se puede modificar la entidad
        /// </summary>
        [HttpGet("{id}/validar-modificar")]
        public async Task<IActionResult> ValidarModificar([FromRoute] int id)
        {
            var query = new ObservacionesQuery();
            var result = await _mediator.Send(query);
            var entity = result.FirstOrDefault(x => x.Id == id);

            if (entity == null)
                return NotFound(new { canExecute = false, errors = new[] { new { code = "NOT_FOUND", message = "Observacion no encontrado" } } });

            // Por ahora retornamos success - la validación real se hace en el dominio
            return Ok(new { canExecute = true, errors = Array.Empty<object>(), validatedAt = DateTime.UtcNow });
        }

        /// <summary>
        /// Pre-valida si se puede eliminar la entidad
        /// </summary>
        [HttpGet("{id}/validar-eliminar")]
        public async Task<IActionResult> ValidarEliminar([FromRoute] int id)
        {
            var query = new ObservacionesQuery();
            var result = await _mediator.Send(query);
            var entity = result.FirstOrDefault(x => x.Id == id);

            if (entity == null)
                return NotFound(new { canExecute = false, errors = new[] { new { code = "NOT_FOUND", message = "Observacion no encontrado" } } });

            // Por ahora retornamos success - la validación real se hace en el dominio
            return Ok(new { canExecute = true, errors = Array.Empty<object>(), validatedAt = DateTime.UtcNow });
        }

        /// <summary>
        /// Pre-valida si se puede crear una nueva entidad
        /// </summary>
        [HttpGet("validar-crear")]
        public IActionResult ValidarCrear()
        {
            // Por ahora retornamos success - la validación real se hace en el dominio
            return Ok(new { canExecute = true, errors = Array.Empty<object>(), validatedAt = DateTime.UtcNow });
        }
    }
}
