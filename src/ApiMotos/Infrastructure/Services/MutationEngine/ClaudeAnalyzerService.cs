using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ApiMotos.Application.Agregates.Mutation.Commands.AnalizarMutacion;
using ApiMotos.Application.Agregates.Mutation.DTOs;

namespace ApiMotos.Infrastructure.Services.MutationEngine;

/// <summary>
/// Servicio que analiza solicitudes de mutación usando Claude Code CLI.
/// Usa la suscripción Claude Max (no consume créditos API).
/// Claude Code se ejecuta desde SiteMotos con acceso directo al proyecto.
/// </summary>
public class ClaudeAnalyzerService : IMutationAnalyzerService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ClaudeAnalyzerService> _logger;
    private readonly string _siteCorePath;

    public ClaudeAnalyzerService(IConfiguration configuration, ILogger<ClaudeAnalyzerService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Resolver path absoluto de SiteMotos
        var sitePath = _configuration["CodeGenerator:SiteProjectPath"] ?? "../SiteMotos";
        _siteCorePath = Path.GetFullPath(sitePath);
        _logger.LogInformation("SiteMotos path: {Path}", _siteCorePath);
    }

    public async Task<Result<MutacionResponseDto>> AnalizarSolicitudAsync(
        string solicitud,
        ContextoActualDto? contexto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var systemPrompt = BuildSystemPrompt();
            var userPrompt = BuildUserPrompt(solicitud, contexto);
            var fullPrompt = $"{systemPrompt}\n\n{userPrompt}";

            _logger.LogInformation("Iniciando análisis de mutación con Claude Code CLI");

            // Detectar OS para ejecutar claude correctamente
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var fileName = isWindows ? "cmd.exe" : "claude";
            var arguments = isWindows
                ? "/c claude --dangerously-skip-permissions -p"
                : "--dangerously-skip-permissions -p";

            _logger.LogInformation("Ejecutando: {FileName} {Arguments}", fileName, arguments);

            var processInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = _siteCorePath, // Ejecutar desde SiteMotos
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            _logger.LogInformation("Working directory: {Dir}", _siteCorePath);

            using var process = new Process { StartInfo = processInfo };

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    outputBuilder.AppendLine(e.Data);
                    _logger.LogDebug("Claude Output: {Output}", e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    errorBuilder.AppendLine(e.Data);
                    _logger.LogWarning("Claude Error: {Error}", e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Enviar prompt a stdin
            await process.StandardInput.WriteAsync(fullPrompt);
            await process.StandardInput.FlushAsync();
            process.StandardInput.Close();

            // Esperar con timeout de 3 minutos
            var timeout = TimeSpan.FromMinutes(3);
            try
            {
                await process.WaitForExitAsync(CancellationToken.None).WaitAsync(timeout);
            }
            catch (TimeoutException)
            {
                process.Kill(true);
                return Result.Fail<MutacionResponseDto>("Claude Code excedió el timeout de 5 minutos");
            }

            var output = outputBuilder.ToString();
            var errors = errorBuilder.ToString();

            if (process.ExitCode != 0)
            {
                _logger.LogError("Claude Code falló con código {ExitCode}. Error: {Error}",
                    process.ExitCode, errors);
                return Result.Fail<MutacionResponseDto>($"Error en Claude Code: {errors}");
            }

            // Extraer JSON de la respuesta
            var jsonContent = ExtractJson(output);

            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                _logger.LogError("No se pudo extraer JSON de la respuesta de Claude");
                _logger.LogDebug("Output completo: {Output}", output);
                return Result.Fail<MutacionResponseDto>("Claude no devolvió JSON válido");
            }

            // Deserializar respuesta
            var mutacionResponse = JsonSerializer.Deserialize<MutacionResponseDto>(
                jsonContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                }
            );

            if (mutacionResponse == null)
                return Result.Fail<MutacionResponseDto>("No se pudo deserializar la respuesta");

            _logger.LogInformation("Análisis de mutación completado exitosamente");
            return Result.Ok(mutacionResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar Claude Code CLI");
            return Result.Fail<MutacionResponseDto>($"Error inesperado: {ex.Message}");
        }
    }

    private string BuildSystemPrompt()
    {
        return """
            Angular 18. Responde SOLO JSON.

            {"mutation_summary":"descripcion","structural_impact":{"frontend_changes":[{"path":"RUTA_EXACTA","action":"modify","code":"CODIGO_COMPLETO","language":"html","description":"cambio"}]}}

            REGLAS: path=ruta exacta del archivo, code=archivo COMPLETO modificado
            """;
    }

    private string BuildUserPrompt(string solicitud, ContextoActualDto? contexto)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine($"CAMBIO: {solicitud}");

        if (contexto != null && !string.IsNullOrEmpty(contexto.RutaActual))
        {
            var rutaParts = contexto.RutaActual.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (rutaParts.Length > 0)
            {
                var modulo = rutaParts[0];

                // Leer SOLO el archivo HTML del componente principal
                var archivo = BuscarArchivoComponente(modulo, "html");
                if (archivo.HasValue)
                {
                    prompt.AppendLine($"\nARCHIVO: {archivo.Value.Path}");
                    prompt.AppendLine($"```html\n{archivo.Value.Content}\n```");
                }
            }
        }

        prompt.AppendLine("\nResponde SOLO JSON.");
        return prompt.ToString();
    }

    private (string Path, string Content)? BuscarArchivoComponente(string modulo, string extension)
    {
        try
        {
            // Buscar en components del módulo
            var componentsPath = Path.Combine(_siteCorePath, $"src/app/modules/{modulo}/components");

            if (!Directory.Exists(componentsPath))
            {
                // Intentar en generated
                componentsPath = Path.Combine(_siteCorePath, $"src/app/modules/generated/components/{modulo}");
            }

            if (!Directory.Exists(componentsPath))
            {
                _logger.LogWarning("No se encontró directorio de componentes para {Modulo}", modulo);
                return null;
            }

            // Buscar el componente -list primero (es el principal)
            var dirs = Directory.GetDirectories(componentsPath);
            var targetDir = dirs.FirstOrDefault(d => d.Contains("-list"))
                ?? dirs.FirstOrDefault(d => d.Contains(modulo))
                ?? dirs.FirstOrDefault();

            if (targetDir == null) return null;

            // Buscar archivo con la extensión
            var pattern = $"*.component.{extension}";
            var files = Directory.GetFiles(targetDir, pattern);
            var file = files.FirstOrDefault();

            if (file == null) return null;

            var relativePath = Path.GetRelativePath(_siteCorePath, file).Replace("\\", "/");
            var content = File.ReadAllText(file);

            // Limitar tamaño
            if (content.Length > 6000)
                content = content[..6000] + "\n<!-- truncado -->";

            _logger.LogInformation("Archivo encontrado: {Path} ({Len} chars)", relativePath, content.Length);
            return (relativePath, content);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error buscando archivo para {Modulo}", modulo);
            return null;
        }
    }

    private static string ExtractJson(string text)
    {
        // Buscar JSON entre ```json y ```
        var jsonStart = text.IndexOf("```json", StringComparison.Ordinal);
        if (jsonStart >= 0)
        {
            jsonStart += 7;
            var jsonEnd = text.IndexOf("```", jsonStart, StringComparison.Ordinal);
            if (jsonEnd > jsonStart)
            {
                return text[jsonStart..jsonEnd].Trim();
            }
        }

        // Buscar JSON entre ``` y ```
        jsonStart = text.IndexOf("```", StringComparison.Ordinal);
        if (jsonStart >= 0)
        {
            jsonStart += 3;
            var jsonEnd = text.IndexOf("```", jsonStart, StringComparison.Ordinal);
            if (jsonEnd > jsonStart)
            {
                return text[jsonStart..jsonEnd].Trim();
            }
        }

        // Buscar primer { y último }
        var firstBrace = text.IndexOf('{');
        var lastBrace = text.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            return text[firstBrace..(lastBrace + 1)].Trim();
        }

        // Asumir que todo el texto es JSON
        return text.Trim();
    }
}
