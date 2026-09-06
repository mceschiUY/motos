using Microsoft.AspNetCore.Mvc;
using ApiMotos.Services.TemplateGenerator;

using Microsoft.AspNetCore.Authorization;
namespace ApiMotos.Controllers
{
    /// <summary>
    /// Controlador para operaciones del sistema como seleccion de carpetas
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SystemController : ControllerBase
    {
        private readonly ILogger<SystemController> _logger;
        private readonly IConfiguration _configuration;

        public SystemController(ILogger<SystemController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Obtiene la configuración de rutas del proyecto con rutas resueltas y validación.
        /// Retorna rutas absolutas calculadas desde rutas relativas en appsettings.json.
        /// </summary>
        [HttpGet("config")]
        public IActionResult GetProjectConfig()
        {
            try
            {
                var errors = new List<string>();

                // Leer configuración y resolver rutas relativas
                var basePath = GeneratorConfig.GetFullPath(_configuration["CodeGenerator:BasePath"] ?? ".");
                var universesOutputPath = GeneratorConfig.GetFullPath(_configuration["CodeGenerator:UniversesOutputPath"] ?? "../universos_generados");
                var frontendProjectPath = GeneratorConfig.GetFullPath(_configuration["CodeGenerator:FrontendProjectPath"] ?? "../zas");
                var siteProjectPath = GeneratorConfig.GetFullPath(_configuration["CodeGenerator:SiteProjectPath"] ?? "../SiteMotos");

                // Validar que las rutas existan
                if (!Directory.Exists(basePath))
                    errors.Add($"BasePath no existe: {basePath}");
                if (!Directory.Exists(universesOutputPath))
                {
                    // Intentar crear la carpeta de universos si no existe
                    try
                    {
                        Directory.CreateDirectory(universesOutputPath);
                        _logger.LogInformation("Creada carpeta de universos: {Path}", universesOutputPath);
                    }
                    catch
                    {
                        errors.Add($"UniversesOutputPath no existe y no se pudo crear: {universesOutputPath}");
                    }
                }

                var config = new
                {
                    // Rutas originales (como están en appsettings.json)
                    rawBasePath = _configuration["CodeGenerator:BasePath"] ?? ".",
                    rawUniversesOutputPath = _configuration["CodeGenerator:UniversesOutputPath"] ?? "../universos_generados",
                    rawFrontendProjectPath = _configuration["CodeGenerator:FrontendProjectPath"] ?? "../zas",
                    rawSiteProjectPath = _configuration["CodeGenerator:SiteProjectPath"] ?? "../SiteMotos",

                    // Rutas resueltas (absolutas)
                    basePath,
                    universesOutputPath,
                    frontendProjectPath,
                    siteProjectPath,

                    // Otros valores
                    projectName = _configuration["CodeGenerator:ProjectName"] ?? "ApiMotos",
                    tablePrefix = _configuration["CodeGenerator:TablePrefix"] ?? "PC_",
                    databaseName = _configuration["CodeGenerator:DatabaseName"] ?? "DESA",

                    // Estado de validación
                    isValid = errors.Count == 0,
                    errors
                };

                if (errors.Count > 0)
                {
                    _logger.LogWarning("Configuración con errores: {Errors}", string.Join(", ", errors));
                }

                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración del proyecto");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lista las subcarpetas de una ruta
        /// </summary>
        [HttpPost("list-folders")]
        public IActionResult ListFolders([FromBody] BrowseFolderRequest request)
        {
            try
            {
                var defaultPath = _configuration["CodeGenerator:BasePath"] ?? "";
                var basePath = request.InitialPath ?? defaultPath;

                if (!Directory.Exists(basePath))
                {
                    return NotFound(new { error = "La ruta no existe" });
                }

                var folders = Directory.GetDirectories(basePath)
                    .Select(d => new DirectoryInfo(d))
                    .Where(d => !d.Attributes.HasFlag(FileAttributes.Hidden))
                    .Select(d => new
                    {
                        name = d.Name,
                        path = d.FullName,
                        hasSubfolders = Directory.GetDirectories(d.FullName).Any()
                    })
                    .OrderBy(f => f.name)
                    .ToList();

                return Ok(new { folders, currentPath = basePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar carpetas");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class BrowseFolderRequest
    {
        public string? InitialPath { get; set; }
    }
}
