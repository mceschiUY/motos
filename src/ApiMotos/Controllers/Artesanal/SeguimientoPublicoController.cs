using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiMotos.Application.Artesanal.Logistica.SeguimientoEnvio;
using ApiMotos.Application.Agregates.Documentos.Queries.ById;
using ApiMotos.Application.Common.Abstractions;

namespace ApiMotos.Controllers.Artesanal
{
    /// <summary>
    /// Seguimiento PÚBLICO del envío: lo abre el cliente de la tienda desde el QR o el link
    /// compartido, sin login. Único endpoint anónimo fuera de auth/configuracion.
    ///
    /// Acceso anónimo: [AllowAnonymous] a nivel controller (no hay FallbackPolicy que exija
    /// usuario) y la query lleva [ZasAllowAnonymous] para que el SecurityBehavior de MediatR
    /// no la rechace. Lo que NO viaja lo recorta la query con Publico=true: precios, costos,
    /// totales, teléfonos y usuarios. Solo GET.
    /// </summary>
    [ApiController]
    [Route("api/publico")]
    [AllowAnonymous]
    public class SeguimientoPublicoController : ControllerBase
    {
        private sealed class Conteo { public int Total { get; set; } }

        private readonly IMediator _mediator;
        private readonly IQueryService _consultas;

        public SeguimientoPublicoController(IMediator mediator, IQueryService consultas)
        {
            _mediator = mediator;
            _consultas = consultas;
        }

        /// <summary>
        /// Foto de producto para la página pública (revisión de escenas 2026-09-12). Solo sirve
        /// documentos que sean la portada de algún producto: cualquier otro id devuelve 404,
        /// así el endpoint anónimo no expone el resto de PC_DOCUMENTOS.
        /// </summary>
        [HttpGet("imagen/{id:int}")]
        [ResponseCache(Duration = 3600)]
        public async Task<IActionResult> Imagen([FromRoute] int id)
        {
            var esPortada = (await _consultas.ConsultarAsync<Conteo>(
                "SELECT COUNT(*) AS Total FROM PC_PRODUCTOS WHERE ImagenPrincipalId = @Id AND Activo = 1", new { Id = id })).FirstOrDefault();
            if (esPortada == null || esPortada.Total == 0)
                return NotFound();
            var documento = await _mediator.Send(new DocumentoByIdQuery(id));
            if (documento?.Contenido == null)
                return NotFound();
            return File(documento.Contenido, documento.MimeType ?? "image/png");
        }

        /// <summary>Seguimiento por código de rastreo (sin distinguir mayúsculas). 404 si no existe.</summary>
        [HttpGet("seguimiento/{codigo}")]
        public async Task<IActionResult> Seguimiento([FromRoute] string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return NotFound("No encontramos un envío con ese código");

            var result = await _mediator.Send(new SeguimientoEnvioQuery(null, codigo, Publico: true));
            if (result == null)
                return NotFound("No encontramos un envío con ese código");
            return Ok(result);
        }
    }
}
