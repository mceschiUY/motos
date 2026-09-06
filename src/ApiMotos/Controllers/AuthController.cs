using MediatR;
using Microsoft.AspNetCore.Mvc;
using ApiMotos.Application.Agregates.Auth.Dtos;
using ApiMotos.Application.Agregates.Auth.Queries.Login;

namespace ApiMotos.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Autentica un usuario y devuelve un token JWT
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Usuario) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Usuario y contraseña son requeridos");
        }

        var result = await _mediator.Send(new LoginQuery(request.Usuario, request.Password));

        if (result.IsFailed)
        {
            var errorMessage = result.Errors.First().Message;

            // Usuario bloqueado = 423 Locked
            if (errorMessage.Contains("bloqueado"))
            {
                return StatusCode(423, new { error = errorMessage });
            }

            // Credenciales incorrectas = 401 Unauthorized
            return Unauthorized(new { error = errorMessage });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Verifica si el token actual es valido
    /// </summary>
    [HttpGet("verify")]
    public ActionResult Verify()
    {
        // Si llegó aquí, el token es válido (el middleware de auth lo validó)
        return Ok(new { valid = true });
    }
}
