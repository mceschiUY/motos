using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ApiMotos.Application.Agregates.Mutation.Commands.AnalizarMutacion;
using ApiMotos.Application.Agregates.Mutation.Commands.EjecutarMutacion;
using ApiMotos.Application.Agregates.Mutation.Commands.RevertirMutacion;
using ApiMotos.Application.Agregates.Mutation.DTOs;
using ApiMotos.Application.Agregates.Mutation.Queries.ObtenerHistorial;
using ApiMotos.Application.Agregates.Mutation.Queries.ObtenerMutacion;
using ApiMotos.Domain.Agregates.Mutation;

using Microsoft.AspNetCore.Authorization;
namespace ApiMotos.Controllers;

/// <summary>
/// Controller para el sistema de mutaciones ZAS.
/// Permite analizar, previsualizar y ejecutar cambios en el código mediante lenguaje natural.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MutacionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMutacionRepositorio _repo;
    private readonly ILogger<MutacionController> _logger;
    private readonly IConfiguration _configuration;

    public MutacionController(
        IMediator mediator,
        IMutacionRepositorio repo,
        ILogger<MutacionController> logger,
        IConfiguration configuration)
    {
        _mediator = mediator;
        _repo = repo;
        _logger = logger;
        _configuration = configuration;
    }

    // =============================================================================
    // FLUJO PRINCIPAL
    // =============================================================================

    /// <summary>
    /// Analiza una solicitud de mutación y genera preview + impacto estructural.
    /// No ejecuta cambios, solo prepara la información para revisión del usuario.
    /// </summary>
    /// <param name="request">Solicitud en lenguaje natural + contexto opcional</param>
    /// <returns>Preview HTML, impacto estructural y auditoría de arquitectura</returns>
    [HttpPost("analyze")]
    public async Task<ActionResult<MutacionResponseDto>> Analyze([FromBody] MutacionRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Solicitud))
            return BadRequest("La solicitud no puede estar vacía");

        _logger.LogInformation("Analizando mutación: {Solicitud}", request.Solicitud[..Math.Min(100, request.Solicitud.Length)]);

        var command = new AnalizarMutacionCommand(request.Solicitud, request.Contexto);
        var result = await _mediator.Send(command);

        if (result.IsFailed)
            return BadRequest(new { Error = result.Errors.First().Message });

        return Ok(result.Value);
    }

    /// <summary>
    /// Analiza una solicitud de mutación con streaming en tiempo real (Server-Sent Events).
    /// Envía eventos mientras se genera el código para mostrar progreso en vivo.
    /// </summary>
    /// <param name="solicitud">Solicitud en lenguaje natural</param>
    /// <param name="contexto">Contexto opcional en formato JSON</param>
    [HttpGet("analyze-stream")]
    public async Task AnalyzeStream(
        [FromQuery] string solicitud,
        [FromQuery] string? contexto = null)
    {
        if (string.IsNullOrWhiteSpace(solicitud))
        {
            Response.StatusCode = 400;
            return;
        }

        _logger.LogInformation("Iniciando análisis con streaming: {Solicitud}", solicitud[..Math.Min(100, solicitud.Length)]);

        // Configurar respuesta SSE (compatible con HTTP/2)
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache, no-store");
        Response.Headers.Append("X-Accel-Buffering", "no");
        // Nota: "Connection: keep-alive" no es necesario en HTTP/2

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true // Para deserializar camelCase -> PascalCase
        };

        try
        {
            // Parsear contexto si existe
            ContextoActualDto? contextoDto = null;
            if (!string.IsNullOrWhiteSpace(contexto))
            {
                try
                {
                    contextoDto = JsonSerializer.Deserialize<ContextoActualDto>(contexto, jsonOptions);
                    _logger.LogInformation("Contexto recibido - Ruta: {Ruta}, Componente: {Componente}",
                        contextoDto?.RutaActual, contextoDto?.ComponenteActual);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Error parseando contexto: {Error}", ex.Message);
                }
            }

            // Enviar evento de inicio
            await SendSseEventAsync("start", new
            {
                type = "start",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                mutacionId = 0,
                message = "Iniciando análisis de mutación..."
            }, jsonOptions);

            // Simular progreso de interpretación
            await SendSseEventAsync("progress", new
            {
                type = "progress",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                phase = "interpreting",
                percentage = 10,
                message = "Interpretando solicitud..."
            }, jsonOptions);

            // Ejecutar el análisis real con heartbeats para mantener la conexión viva
            var command = new AnalizarMutacionCommand(solicitud, contextoDto);

            // Iniciar tarea de heartbeat
            var cts = new CancellationTokenSource();
            var heartbeatTask = SendHeartbeatsAsync(jsonOptions, cts.Token);

            FluentResults.Result<MutacionResponseDto> result;
            try
            {
                // Enviar progreso antes de iniciar Claude
                await SendSseEventAsync("progress", new
                {
                    type = "progress",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    phase = "analyzing",
                    percentage = 20,
                    message = "Iniciando análisis con Claude..."
                }, jsonOptions);

                result = await _mediator.Send(command);
            }
            finally
            {
                // Detener heartbeats
                cts.Cancel();
                try { await heartbeatTask; } catch { }
            }

            if (result.IsFailed)
            {
                await SendSseEventAsync("error", new
                {
                    type = "error",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    code = "ANALYSIS_FAILED",
                    message = result.Errors.First().Message
                }, jsonOptions);
                return;
            }

            var response = result.Value;

            // Enviar progreso de generación
            await SendSseEventAsync("progress", new
            {
                type = "progress",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                phase = "generating",
                percentage = 50,
                message = "Generando código..."
            }, jsonOptions);

            // Enviar archivos generados uno por uno
            var fileIndex = 0;
            var totalFiles = (response.StructuralImpact?.BackendChanges?.Count ?? 0) +
                           (response.StructuralImpact?.FrontendChanges?.Count ?? 0);

            // Procesar archivos backend
            if (response.StructuralImpact?.BackendChanges != null)
            {
                foreach (var cambio in response.StructuralImpact.BackendChanges)
                {
                    fileIndex++;
                    var progress = 50 + (int)(40.0 * fileIndex / Math.Max(totalFiles, 1));

                    var fileId = $"file_{fileIndex}";
                    var fileName = Path.GetFileName(cambio.Path);

                    // Evento de inicio de archivo
                    await SendSseEventAsync("file_start", new
                    {
                        type = "file_start",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        fileId,
                        fileName,
                        filePath = cambio.Path,
                        language = MapLanguage(cambio.Language),
                        layer = "backend"
                    }, jsonOptions);

                    // Enviar contenido en chunks
                    await SendFileContentInChunksAsync(fileId, cambio.Code, jsonOptions);

                    // Evento de archivo completado
                    await SendSseEventAsync("file_complete", new
                    {
                        type = "file_complete",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        fileId,
                        totalLines = cambio.Code?.Split('\n').Length ?? 0,
                        action = cambio.Action.ToLower()
                    }, jsonOptions);

                    await SendSseEventAsync("progress", new
                    {
                        type = "progress",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        phase = "generating",
                        percentage = progress,
                        message = $"Generando {fileName}..."
                    }, jsonOptions);
                }
            }

            // Procesar archivos frontend
            if (response.StructuralImpact?.FrontendChanges != null)
            {
                foreach (var cambio in response.StructuralImpact.FrontendChanges)
                {
                    fileIndex++;
                    var progress = 50 + (int)(40.0 * fileIndex / Math.Max(totalFiles, 1));

                    var fileId = $"file_{fileIndex}";
                    var fileName = Path.GetFileName(cambio.Path);

                    // Evento de inicio de archivo
                    await SendSseEventAsync("file_start", new
                    {
                        type = "file_start",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        fileId,
                        fileName,
                        filePath = cambio.Path,
                        language = MapLanguage(cambio.Language),
                        layer = "frontend"
                    }, jsonOptions);

                    // Enviar contenido en chunks
                    await SendFileContentInChunksAsync(fileId, cambio.Code, jsonOptions);

                    // Evento de archivo completado
                    await SendSseEventAsync("file_complete", new
                    {
                        type = "file_complete",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        fileId,
                        totalLines = cambio.Code?.Split('\n').Length ?? 0,
                        action = cambio.Action.ToLower()
                    }, jsonOptions);

                    await SendSseEventAsync("progress", new
                    {
                        type = "progress",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        phase = "generating",
                        percentage = progress,
                        message = $"Generando {fileName}..."
                    }, jsonOptions);
                }
            }

            // Enviar preview HTML
            if (!string.IsNullOrWhiteSpace(response.PreviewHtml))
            {
                await SendSseEventAsync("preview_html", new
                {
                    type = "preview_html",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    html = response.PreviewHtml
                }, jsonOptions);
            }

            // Evento de finalización
            await SendSseEventAsync("progress", new
            {
                type = "progress",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                phase = "finalizing",
                percentage = 95,
                message = "Finalizando análisis..."
            }, jsonOptions);

            await Task.Delay(200);

            // Evento de análisis completado
            await SendSseEventAsync("analysis_complete", new
            {
                type = "analysis_complete",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                mutacionId = response.MutacionId,
                summary = response.MutationSummary,
                architectureAudit = new
                {
                    isCompliant = response.ArchitectureAudit?.IsCompliant ?? true,
                    compliancePercentage = response.ArchitectureAudit?.CompliancePercentage ?? 100,
                    warnings = response.ArchitectureAudit?.Warnings ?? new List<string>(),
                    recommendations = response.ArchitectureAudit?.Recommendations ?? new List<string>(),
                    message = response.ArchitectureAudit?.Message
                },
                totalFiles = fileIndex
            }, jsonOptions);

            _logger.LogInformation("Análisis con streaming completado. MutacionId: {Id}", response.MutacionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante análisis con streaming");

            await SendSseEventAsync("error", new
            {
                type = "error",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                code = "STREAM_ERROR",
                message = ex.Message
            }, jsonOptions);
        }
    }

    /// <summary>
    /// Envía un evento SSE al cliente
    /// </summary>
    private async Task SendSseEventAsync(string eventName, object data, JsonSerializerOptions options)
    {
        var json = JsonSerializer.Serialize(data, options);
        await Response.WriteAsync($"event: {eventName}\n");
        await Response.WriteAsync($"data: {json}\n\n");
        await Response.Body.FlushAsync();
    }

    /// <summary>
    /// Envía heartbeats periódicos para mantener la conexión SSE viva mientras Claude procesa
    /// </summary>
    private async Task SendHeartbeatsAsync(JsonSerializerOptions options, CancellationToken cancellationToken)
    {
        var progressValues = new[] { 25, 30, 35, 40, 45 };
        var messages = new[]
        {
            "Claude está analizando la solicitud...",
            "Evaluando impacto en la arquitectura...",
            "Generando código...",
            "Procesando cambios...",
            "Casi listo..."
        };

        var index = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(5000, cancellationToken); // Cada 5 segundos

                if (cancellationToken.IsCancellationRequested) break;

                await SendSseEventAsync("progress", new
                {
                    type = "progress",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    phase = "analyzing",
                    percentage = progressValues[Math.Min(index, progressValues.Length - 1)],
                    message = messages[Math.Min(index, messages.Length - 1)]
                }, options);

                index++;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error enviando heartbeat SSE");
                break;
            }
        }
    }

    /// <summary>
    /// Envía el contenido de un archivo en chunks para simular streaming de código
    /// </summary>
    private async Task SendFileContentInChunksAsync(string fileId, string? content, JsonSerializerOptions options)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            await SendSseEventAsync("file_chunk", new
            {
                type = "file_chunk",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                fileId,
                content = "",
                isComplete = true
            }, options);
            return;
        }

        // Dividir en líneas y enviar en grupos
        var lines = content.Split('\n');
        var chunkSize = Math.Max(5, lines.Length / 10); // Enviar en ~10 chunks
        var currentChunk = new List<string>();

        for (int i = 0; i < lines.Length; i++)
        {
            currentChunk.Add(lines[i]);

            if (currentChunk.Count >= chunkSize || i == lines.Length - 1)
            {
                var chunkContent = string.Join('\n', currentChunk);
                var isComplete = i == lines.Length - 1;

                await SendSseEventAsync("file_chunk", new
                {
                    type = "file_chunk",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    fileId,
                    content = chunkContent + (isComplete ? "" : "\n"),
                    isComplete
                }, options);

                currentChunk.Clear();

                // Pequeña pausa para simular escritura gradual
                if (!isComplete)
                {
                    await Task.Delay(50);
                }
            }
        }
    }

    /// <summary>
    /// Mapea el nombre del lenguaje al formato esperado por el frontend
    /// </summary>
    private static string MapLanguage(string? language)
    {
        return language?.ToLower() switch
        {
            "c#" or "csharp" => "csharp",
            "typescript" or "ts" => "typescript",
            "html" => "html",
            "scss" or "css" => "scss",
            "sql" => "sql",
            _ => "plaintext"
        };
    }

    /// <summary>
    /// Ejecuta una mutación previamente analizada.
    /// Escribe físicamente los archivos generados.
    /// </summary>
    /// <param name="id">ID de la mutación a ejecutar</param>
    /// <returns>Resultado de la ejecución con archivos afectados</returns>
    [HttpPost("{id}/execute")]
    public async Task<ActionResult<EjecucionResultadoDto>> Execute(int id)
    {
        _logger.LogInformation("Ejecutando mutación {Id}", id);

        var command = new EjecutarMutacionCommand(id);
        var result = await _mediator.Send(command);

        if (result.IsFailed)
            return BadRequest(new { Error = result.Errors.First().Message });

        return Ok(result.Value);
    }

    /// <summary>
    /// Ejecuta cambios directamente sin pasar por la base de datos.
    /// Recibe los archivos a escribir y los aplica inmediatamente.
    /// </summary>
    [HttpPost("execute-direct")]
    public async Task<ActionResult<EjecucionResultadoDto>> ExecuteDirect([FromBody] ExecuteDirectRequest request)
    {
        _logger.LogInformation("Ejecutando cambios directos: {Count} archivos", request.Changes?.Count ?? 0);

        if (request.Changes == null || request.Changes.Count == 0)
            return BadRequest(new { Error = "No hay cambios para aplicar" });

        var sitePath = _configuration["CodeGenerator:SiteProjectPath"] ?? "../SiteMotos";
        var siteFullPath = Path.GetFullPath(sitePath);

        var archivosAfectados = new List<string>();
        var archivosCreados = 0;
        var archivosModificados = 0;

        try
        {
            foreach (var change in request.Changes)
            {
                if (string.IsNullOrEmpty(change.Path) || string.IsNullOrEmpty(change.Code))
                {
                    _logger.LogWarning("Saltando cambio - Path o Code vacío");
                    continue;
                }

                var rutaCompleta = Path.Combine(siteFullPath, change.Path);
                _logger.LogInformation("Escribiendo: {Ruta}", rutaCompleta);

                // Crear directorio si no existe
                var directorio = Path.GetDirectoryName(rutaCompleta);
                if (!string.IsNullOrEmpty(directorio) && !System.IO.Directory.Exists(directorio))
                {
                    System.IO.Directory.CreateDirectory(directorio);
                }

                var existia = System.IO.File.Exists(rutaCompleta);
                await System.IO.File.WriteAllTextAsync(rutaCompleta, change.Code);

                if (existia)
                    archivosModificados++;
                else
                    archivosCreados++;

                archivosAfectados.Add(change.Path);
                _logger.LogInformation("Archivo {Accion}: {Ruta}", existia ? "modificado" : "creado", rutaCompleta);
            }

            return Ok(new EjecucionResultadoDto
            {
                Success = true,
                MutacionId = 0,
                ArchivosCreados = archivosCreados,
                ArchivosModificados = archivosModificados,
                ArchivosEliminados = 0,
                ArchivosAfectados = archivosAfectados
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando cambios directos");
            return BadRequest(new { Error = $"Error al escribir archivos: {ex.Message}" });
        }
    }

    /// <summary>
    /// Revierte archivos usando git checkout.
    /// Restaura los archivos a su estado en git (antes de los cambios).
    /// </summary>
    [HttpPost("revert-files")]
    public async Task<ActionResult> RevertFiles([FromBody] RevertFilesRequest request)
    {
        _logger.LogInformation("Revirtiendo archivos: {Count}", request.Paths?.Count ?? 0);

        if (request.Paths == null || request.Paths.Count == 0)
            return BadRequest(new { Error = "No hay archivos para revertir" });

        var sitePath = _configuration["CodeGenerator:SiteProjectPath"] ?? "../SiteMotos";
        var siteFullPath = Path.GetFullPath(sitePath);

        try
        {
            foreach (var relativePath in request.Paths)
            {
                var fullPath = Path.Combine(siteFullPath, relativePath);
                _logger.LogInformation("Revirtiendo: {Path}", fullPath);

                // Usar git checkout para revertir el archivo
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"checkout -- \"{relativePath}\"",
                    WorkingDirectory = siteFullPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(processInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    var error = await process.StandardError.ReadToEndAsync();

                    if (process.ExitCode != 0)
                    {
                        _logger.LogWarning("Git checkout falló para {Path}: {Error}", relativePath, error);
                    }
                    else
                    {
                        _logger.LogInformation("Archivo revertido: {Path}", relativePath);
                    }
                }
            }

            return Ok(new { Success = true, Message = $"{request.Paths.Count} archivo(s) revertido(s)" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revirtiendo archivos");
            return BadRequest(new { Error = $"Error al revertir: {ex.Message}" });
        }
    }

    /// <summary>
    /// Revierte una mutación ejecutada (rollback).
    /// Restaura los archivos a su estado anterior.
    /// </summary>
    /// <param name="id">ID de la mutación a revertir</param>
    /// <returns>Éxito o error del rollback</returns>
    [HttpPost("{id}/rollback")]
    public async Task<ActionResult> Rollback(int id)
    {
        _logger.LogInformation("Revirtiendo mutación {Id}", id);

        var command = new RevertirMutacionCommand(id);
        var result = await _mediator.Send(command);

        if (result.IsFailed)
            return BadRequest(new { Error = result.Errors.First().Message });

        return Ok(new { Success = true, Message = "Mutación revertida exitosamente" });
    }

    // =============================================================================
    // CONSULTAS
    // =============================================================================

    /// <summary>
    /// Obtiene una mutación por ID con todos sus detalles
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MutacionResponseDto>> GetById(int id)
    {
        var query = new ObtenerMutacionQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsFailed)
            return NotFound(new { Error = result.Errors.First().Message });

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene el historial de mutaciones con paginación
    /// </summary>
    [HttpGet("historial")]
    public async Task<ActionResult<PaginatedResult<MutacionHistorialDto>>> GetHistorial(
        [FromQuery] int? usuarioId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new ObtenerHistorialQuery(usuarioId, page, pageSize);
        var result = await _mediator.Send(query);

        if (result.IsFailed)
            return BadRequest(new { Error = result.Errors.First().Message });

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene las últimas mutaciones
    /// </summary>
    [HttpGet("recientes")]
    public async Task<ActionResult<IEnumerable<MutacionHistorialDto>>> GetRecientes([FromQuery] int cantidad = 10)
    {
        var mutaciones = await _repo.ObtenerUltimasAsync(cantidad);

        var dtos = mutaciones.Select(m => new MutacionHistorialDto
        {
            Id = m.Id,
            SolicitudOriginal = m.SolicitudOriginal,
            ResumenTecnico = m.ResumenTecnico,
            Estado = m.Estado,
            FechaSolicitud = m.FechaSolicitud,
            FechaEjecucion = m.FechaEjecucion,
            UsuarioNombre = m.UsuarioNombre,
            ArchitectureCompliance = m.ArchitectureCompliance,
            CantidadArchivos = m.Archivos.Count
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene mutaciones por estado
    /// </summary>
    [HttpGet("estado/{estado}")]
    public async Task<ActionResult<IEnumerable<MutacionHistorialDto>>> GetByEstado(MutacionEstado estado)
    {
        var mutaciones = await _repo.ObtenerPorEstadoAsync(estado);

        var dtos = mutaciones.Select(m => new MutacionHistorialDto
        {
            Id = m.Id,
            SolicitudOriginal = m.SolicitudOriginal,
            ResumenTecnico = m.ResumenTecnico,
            Estado = m.Estado,
            FechaSolicitud = m.FechaSolicitud,
            FechaEjecucion = m.FechaEjecucion,
            UsuarioNombre = m.UsuarioNombre,
            ArchitectureCompliance = m.ArchitectureCompliance,
            CantidadArchivos = m.Archivos.Count
        });

        return Ok(dtos);
    }

    // =============================================================================
    // ESTADÍSTICAS
    // =============================================================================

    /// <summary>
    /// Obtiene estadísticas de mutaciones para el dashboard
    /// </summary>
    [HttpGet("estadisticas")]
    public async Task<ActionResult<MutacionEstadisticasDto>> GetEstadisticas(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        var stats = await _repo.ObtenerEstadisticasAsync(desde, hasta);
        var porEstado = await _repo.ContarPorEstadoAsync();

        return Ok(new MutacionEstadisticasDto
        {
            TotalMutaciones = stats.TotalMutaciones,
            MutacionesEjecutadas = stats.MutacionesEjecutadas,
            MutacionesFallidas = stats.MutacionesFallidas,
            MutacionesRevertidas = stats.MutacionesRevertidas,
            PromedioCompliance = Math.Round(stats.PromedioCompliance, 1),
            ArchivosGenerados = stats.ArchivosGenerados,
            ArchivosModificados = stats.ArchivosModificados,
            PorEstado = porEstado.ToDictionary(
                kv => kv.Key.ToString(),
                kv => kv.Value
            )
        });
    }

    /// <summary>
    /// Obtiene los estados disponibles para filtros
    /// </summary>
    [HttpGet("estados")]
    public ActionResult<IEnumerable<object>> GetEstados()
    {
        return Ok(Enum.GetValues<MutacionEstado>().Select(e => new
        {
            Value = (int)e,
            Name = e.ToString(),
            Label = e switch
            {
                MutacionEstado.Pendiente => "Pendiente",
                MutacionEstado.Analizada => "Analizada",
                MutacionEstado.Previsualizada => "Previsualizada",
                MutacionEstado.Ejecutada => "Ejecutada",
                MutacionEstado.Fallida => "Fallida",
                MutacionEstado.Revertida => "Revertida",
                _ => e.ToString()
            }
        }));
    }
}

// =============================================================================
// DTOs ADICIONALES
// =============================================================================

public class MutacionEstadisticasDto
{
    public int TotalMutaciones { get; set; }
    public int MutacionesEjecutadas { get; set; }
    public int MutacionesFallidas { get; set; }
    public int MutacionesRevertidas { get; set; }
    public double PromedioCompliance { get; set; }
    public int ArchivosGenerados { get; set; }
    public int ArchivosModificados { get; set; }
    public Dictionary<string, int> PorEstado { get; set; } = new();
}

/// <summary>
/// Request para ejecutar cambios directamente sin pasar por BD
/// </summary>
public class ExecuteDirectRequest
{
    public List<DirectChangeDto> Changes { get; set; } = new();
}

public class DirectChangeDto
{
    public string Path { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Action { get; set; } = "modify";
}

/// <summary>
/// Request para revertir archivos usando git checkout
/// </summary>
public class RevertFilesRequest
{
    public List<string> Paths { get; set; } = new();
}
