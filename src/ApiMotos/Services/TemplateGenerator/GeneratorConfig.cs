namespace ApiMotos.Services.TemplateGenerator
{
    /// <summary>
    /// Configuración centralizada para el generador de código.
    /// Se lee desde appsettings.json sección "CodeGenerator"
    /// Usa rutas relativas para compatibilidad cross-platform (Windows/Ubuntu)
    /// </summary>
    public class GeneratorConfig
    {
        /// <summary>
        /// Nombre del proyecto destino (ej: "ApiResidencialCRM", "ApiMotos")
        /// Se usa para paths de archivos y namespaces
        /// </summary>
        public string ProjectName { get; set; } = "ApiMotos";

        /// <summary>
        /// Namespace base del proyecto (ej: "ApiResidencialCRM")
        /// Si está vacío, usa el ProjectName
        /// </summary>
        public string Namespace { get; set; } = "";

        /// <summary>
        /// Prefijo para nombres de tablas en la base de datos (ej: "SU_", "PC_", "MU_")
        /// </summary>
        public string TablePrefix { get; set; } = "PC_";

        /// <summary>
        /// Ruta base donde se encuentran los proyectos plantilla.
        /// Puede ser relativa (ej: ".") o absoluta.
        /// </summary>
        public string BasePath { get; set; } = ".";

        /// <summary>
        /// Ruta donde se generan los universos nuevos.
        /// Puede ser relativa (ej: "../universos_generados") o absoluta.
        /// </summary>
        public string UniversesOutputPath { get; set; } = "../universos_generados";

        /// <summary>
        /// Nombre de la base de datos (ej: "SU", "MU")
        /// Se usa para el prefijo de tablas si TablePrefix está vacío
        /// </summary>
        public string DatabaseName { get; set; } = "DESA";

        /// <summary>
        /// Ruta del proyecto frontend Angular (Kosmos Generator).
        /// Puede ser relativa o absoluta.
        /// </summary>
        public string FrontendProjectPath { get; set; } = "../zas";

        /// <summary>
        /// Ruta del proyecto sitio generado (donde se crean los componentes nuevos).
        /// Puede ser relativa o absoluta.
        /// </summary>
        public string SiteProjectPath { get; set; } = "../SiteMotos";

        /// <summary>
        /// Indica si se usa un proyecto separado para el sitio generado
        /// </summary>
        public bool UseSeparateSiteProject { get; set; } = true;

        /// <summary>
        /// Obtiene el namespace efectivo (usa ProjectName si Namespace está vacío)
        /// </summary>
        public string EffectiveNamespace => string.IsNullOrEmpty(Namespace) ? ProjectName : Namespace;

        /// <summary>
        /// Obtiene el prefijo de tabla efectivo (usa DatabaseName + "_" si TablePrefix está vacío)
        /// </summary>
        public string EffectiveTablePrefix => string.IsNullOrEmpty(TablePrefix) ? $"{DatabaseName}_" : TablePrefix;

        /// <summary>
        /// Obtiene la ruta completa del proyecto backend (resuelve rutas relativas)
        /// </summary>
        public string BackendProjectPath => Path.Combine(GetFullPath(BasePath), ProjectName);

        /// <summary>
        /// Obtiene la ruta del proyecto frontend donde se generan los componentes
        /// Si UseSeparateSiteProject es true, usa SiteProjectPath, sino usa FrontendProjectPath
        /// </summary>
        public string EffectiveFrontendPath => GetFullPath(UseSeparateSiteProject ? SiteProjectPath : FrontendProjectPath);

        /// <summary>
        /// Obtiene la ruta completa de UniversesOutputPath (resuelve rutas relativas)
        /// </summary>
        public string EffectiveUniversesOutputPath => GetFullPath(UniversesOutputPath);

        /// <summary>
        /// Obtiene la ruta completa de BasePath (resuelve rutas relativas)
        /// </summary>
        public string EffectiveBasePath => GetFullPath(BasePath);

        /// <summary>
        /// Convierte una ruta relativa a absoluta.
        /// Si ya es absoluta, la retorna sin cambios.
        /// </summary>
        public static string GetFullPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return Directory.GetCurrentDirectory();

            if (Path.IsPathRooted(path))
                return path;

            return Path.GetFullPath(path, Directory.GetCurrentDirectory());
        }
    }
}
