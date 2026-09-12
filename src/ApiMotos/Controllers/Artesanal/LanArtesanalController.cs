using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiMotos.Application.Artesanal.Comun;

namespace ApiMotos.Controllers.Artesanal
{
    /// <summary>
    /// "Abrir en el celular" (plan Etapa F2): devuelve las IPs de red local de la máquina que
    /// corre la API para que el front arme el QR con la URL del sitio. Solo en Development:
    /// en producción responde 404, ahí no hay nada que descubrir.
    /// </summary>
    [ApiController]
    [Route("api/artesanal")]
    [Authorize]
    public class LanArtesanalController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public LanArtesanalController(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>IPs IPv4 privadas de esta máquina y el puerto HTTP de la API, para la URL del celular.</summary>
        [HttpGet("lan")]
        public IActionResult Lan()
        {
            if (!_env.IsDevelopment()) return NotFound();
            var puertoApi = Request.Host.Port ?? 5100;
            return Ok(new { ips = RedLocal.DireccionesIPv4(), puertoApi, host = Environment.MachineName });
        }
    }
}
