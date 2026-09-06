using MediatR;
using Microsoft.AspNetCore.Mvc;
using ApiMotos.Application.Agregates.Documentos.Commands.Cargar;
using ApiMotos.Application.Agregates.Documentos.Commands.Eliminar;
using ApiMotos.Application.Agregates.Documentos.Queries.Documentos;
using ApiMotos.Application.Agregates.Documentos.Queries.ById;
using ApiMotos.Application.Agregates.Documentos.Queries.ByRelacion;

using Microsoft.AspNetCore.Authorization;
namespace ApiMotos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DocumentosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtiene todos los documentos (sin contenido para optimizar)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<DocumentosDto>>> GetAll()
        {
            var result = await _mediator.Send(new DocumentosQuery());
            return Ok(result);
        }

        /// <summary>
        /// Obtiene un documento por ID (con contenido para descarga)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentoContenidoDto>> GetById(int id)
        {
            var result = await _mediator.Send(new DocumentoByIdQuery(id));
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Obtiene documentos filtrados por entidad relacionada
        /// </summary>
        [HttpGet("by-relacion")]
        public async Task<ActionResult<List<DocumentosDto>>> GetByRelacion(
            [FromQuery] int relacionId,
            [FromQuery] string relacionNombre)
        {
            var result = await _mediator.Send(new DocumentosByRelacionQuery(relacionId, relacionNombre));
            return Ok(result);
        }

        /// <summary>
        /// Descarga un documento como archivo
        /// </summary>
        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var documento = await _mediator.Send(new DocumentoByIdQuery(id));
            if (documento == null)
                return NotFound();

            return File(documento.Contenido, documento.MimeType, $"{documento.Nombre}{documento.Extension}");
        }

        /// <summary>
        /// Sube un nuevo documento
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult<int>> Upload(
            [FromForm] IFormFile file,
            [FromForm] int relacionId,
            [FromForm] string relacionNombre)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No se proporcionó un archivo");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var extension = Path.GetExtension(file.FileName);
            var nombre = Path.GetFileNameWithoutExtension(file.FileName);

            var command = new CargarDocumentoCommand(
                nombre,
                extension,
                memoryStream.ToArray(),
                file.ContentType,
                relacionId,
                relacionNombre
            );

            var result = await _mediator.Send(command);

            if (result.IsFailed)
                return BadRequest(result.Errors.First().Message);

            return Ok(result.Value);
        }

        /// <summary>
        /// Elimina un documento
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<int>> Delete(int id)
        {
            var result = await _mediator.Send(new EliminarDocumentoCommand(id));

            if (result.IsFailed)
                return BadRequest(result.Errors.First().Message);

            return Ok(result.Value);
        }
    }
}
