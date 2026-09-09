using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiMotos.Application.Agregates.Vendedores.Commands.Crear;
using ApiMotos.Application.Agregates.Vendedores.Commands.Modificar;
using ApiMotos.Application.Agregates.Vendedores.Commands.Eliminar;
using ApiMotos.Application.Agregates.Vendedores.Queries.Vendedores;
using ApiMotos.Application.Agregates.Vendedores.Queries.Resumen;
using ApiMotos.Application.Agregates.Vendedores.Queries.Buscar;
using ApiMotos.Application.Agregates.Vendedores.Queries.PorUsuario;

namespace ApiMotos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VendedorController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VendedorController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? skip = null, [FromQuery] int? take = null)
        {
            var result = await _mediator.Send(new VendedoresQuery(Skip: skip, Take: take));
            return Ok(result);
        }

        /// <summary>Vendedor del usuario logueado: `PC_VENDEDORES.Usuario` = claim Name del JWT
        /// (case-insensitive). 404 si el usuario no tiene vendedor asociado (filtro "mis clientes").</summary>
        [HttpGet("mio")]
        public async Task<IActionResult> Mio()
        {
            var usuario = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(usuario))
                return NotFound("El usuario autenticado no tiene nombre de login en el token");
            var item = await _mediator.Send(new VendedorPorUsuarioQuery(usuario));
            if (item == null)
                return NotFound($"El usuario '{usuario}' no tiene un vendedor asociado");
            return Ok(item);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var result = await _mediator.Send(new VendedoresQuery(Id: id));
            var item = result.FirstOrDefault();
            if (item == null)
                return NotFound($"Vendedor con ID {id} no encontrado");
            return Ok(item);
        }

        [HttpGet("resumen")]
        public async Task<IActionResult> Resumen()
        {
            var result = await _mediator.Send(new VendedoresResumenQuery());
            return Ok(result);
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string q)
        {
            var result = await _mediator.Send(new VendedoresBuscarQuery(q ?? ""));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearVendedorCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(new { id = result.Value });
            return Rechazo(result.Errors.Select(e => e.Message));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ModificarVendedorCommand command)
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
            var result = await _mediator.Send(new EliminarVendedorCommand(id));
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
