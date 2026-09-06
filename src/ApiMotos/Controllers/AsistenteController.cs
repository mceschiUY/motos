using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiMotos.Application.Agregates.Asistente.Commands;
using ApiMotos.Application.Agregates.Asistente.Queries;

namespace ApiMotos.Controllers;

/// <summary>
/// Asistente de voz del producto (capa OPCIONAL: sin API key configurada no existe).
/// El controller es finito a propósito — hexagonal: la lógica vive en Application y
/// el proveedor concreto en Infrastructure.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AsistenteController : ControllerBase
{
    private readonly IMediator _mediator;

    public AsistenteController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>¿Este producto tiene asistente de voz? (el front decide si muestra el botón)</summary>
    [HttpGet("estado")]
    public async Task<IActionResult> Estado()
    {
        var result = await _mediator.Send(new EstadoAsistenteQuery());
        return Ok(result);
    }

    /// <summary>Abre una sesión efímera de voz (client secret de corta vida para el navegador).</summary>
    [HttpPost("sesion")]
    public async Task<IActionResult> Sesion()
    {
        var result = await _mediator.Send(new CrearSesionAsistenteCommand());
        if (result.IsFailed)
            return StatusCode(503, new { message = result.Errors.First().Message });
        return Ok(result.Value);
    }
}
