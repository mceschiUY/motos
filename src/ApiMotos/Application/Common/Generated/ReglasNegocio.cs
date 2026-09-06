using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dapper;
using FluentResults;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ApiMotos.Application.Common.Generated
{
    // ─── Banco de Reglas (reglas-dato) ───────────────────────────────────────
    // El generador emite reglas-negocio.json (nombres de tabla/columna/propiedad
    // YA RESUELTOS — el motor no deriva nombres en runtime, solo ejecuta).
    // CrudHooks invoca al ejecutor vía implementación explícita de interfaz,
    // así el motor corre SIEMPRE, haya o no overrides en {Entidad}Hooks.
    // Contrato de perillas y mensajes: BANCO-REGLAS.md (raíz del repo fábrica).

    public class ReglasNegocioArchivo
    {
        public int Version { get; set; } = 1;
        public string? Universo { get; set; }
        public List<ReglaNegocioData> Reglas { get; set; } = new();
    }

    public class ReglaNegocioData
    {
        public string Id { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string Entidad { get; set; } = "";
        public List<string> Operaciones { get; set; } = new();
        public string Enforcement { get; set; } = "bloquear";
        public string Mensaje { get; set; } = "";
        public ReglaPerillas Perillas { get; set; } = new();
    }

    public class ReglaPerillas
    {
        // unicidad_compuesta
        public string? Tabla { get; set; }
        public string? ColumnaId { get; set; }
        public List<ReglaCampo>? Campos { get; set; }

        // formato / no_pasado
        public string? Propiedad { get; set; }
        public string? Formato { get; set; }
        public string? EjemploValido { get; set; }
        public string? EjemploInvalido { get; set; }

        // consistencia_campos
        public string? PropiedadA { get; set; }
        public string? Operador { get; set; }
        public string? PropiedadB { get; set; }

        // autorizacion_rol
        public List<string>? ExigeRol { get; set; }
        public List<string>? AplicaARol { get; set; }
        /// <summary>v2: limita la regla a UNA transición de ciclo (nombre Pascal del
        /// handler de acción, ej. "PasarAAprobada"). null = gobierna la operación entera.</summary>
        public string? Accion { get; set; }

        // proteccion_referencial
        public string? TablaHija { get; set; }
        public string? FkColumna { get; set; }
        /// <summary>Columna de estado del hijo (condición opcional). Sin ella, la regla
        /// bloquea con CUALQUIER hijo (mensaje propio; el FK SQL sigue de trinchera).</summary>
        public string? EstadoColumna { get; set; }
        public List<string>? EstadosBloqueantes { get; set; }
    }

    public class ReglaCampo
    {
        public string Columna { get; set; } = "";
        public string Propiedad { get; set; } = "";
    }

    public interface IReglasNegocioBanco
    {
        IReadOnlyList<ReglaNegocioData> Para(string entidad, string operacion);
    }

    /// <summary>
    /// Carga reglas-negocio.json de la raíz del API (singleton). Archivo ausente =
    /// banco vacío (proyectos sin reglas-dato se comportan exactamente igual que antes).
    /// </summary>
    public class ReglasNegocioBanco : IReglasNegocioBanco
    {
        private static readonly Regex IdentificadorSeguro = new("^[A-Za-z0-9_]+$", RegexOptions.Compiled);
        private readonly Dictionary<string, List<ReglaNegocioData>> _porEntidad = new(StringComparer.OrdinalIgnoreCase);

        public ReglasNegocioBanco(IHostEnvironment env, ILogger<ReglasNegocioBanco> logger)
        {
            var path = Path.Combine(env.ContentRootPath, "reglas-negocio.json");
            if (!File.Exists(path))
            {
                logger.LogInformation("[ReglasNegocio] sin reglas-negocio.json: banco vacío");
                return;
            }

            try
            {
                var archivo = JsonSerializer.Deserialize<ReglasNegocioArchivo>(File.ReadAllText(path),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                foreach (var regla in archivo?.Reglas ?? new List<ReglaNegocioData>())
                {
                    if (!IdentificadoresValidos(regla))
                    {
                        logger.LogWarning("[ReglasNegocio] {Regla} descartada: identificadores inválidos", regla.Id);
                        continue;
                    }
                    if (!_porEntidad.TryGetValue(regla.Entidad, out var lista))
                        _porEntidad[regla.Entidad] = lista = new List<ReglaNegocioData>();
                    lista.Add(regla);
                }

                logger.LogInformation("[ReglasNegocio] banco cargado: {Total} reglas-dato en {Entidades} entidades",
                    _porEntidad.Values.Sum(l => l.Count), _porEntidad.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[ReglasNegocio] reglas-negocio.json ilegible: banco vacío");
            }
        }

        private static bool IdentificadoresValidos(ReglaNegocioData regla)
        {
            var ids = new List<string?>
            {
                regla.Perillas.Tabla, regla.Perillas.ColumnaId,
                regla.Perillas.TablaHija, regla.Perillas.FkColumna, regla.Perillas.EstadoColumna,
            };
            if (regla.Perillas.Campos != null)
                foreach (var c in regla.Perillas.Campos) { ids.Add(c.Columna); ids.Add(c.Propiedad); }
            return ids.Where(i => i != null).All(i => IdentificadorSeguro.IsMatch(i!));
        }

        public IReadOnlyList<ReglaNegocioData> Para(string entidad, string operacion)
        {
            if (!_porEntidad.TryGetValue(entidad, out var lista))
                return Array.Empty<ReglaNegocioData>();
            return lista.Where(r => r.Operaciones.Contains(operacion, StringComparer.OrdinalIgnoreCase)).ToList();
        }
    }

    public interface IReglasNegocioEjecutor
    {
        /// <summary>`accion` solo viene en operacion "accion": nombre Pascal del handler
        /// de transición (ej. "PasarAAprobada") para reglas acotadas a una transición.</summary>
        Task<Result> Ejecutar(string entidad, string operacion, object comando, int? idExcluir, CancellationToken ct, string? accion = null);
    }

    public class ReglasNegocioEjecutor : IReglasNegocioEjecutor
    {
        private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> PropCache = new();
        private readonly IReglasNegocioBanco _banco;
        private readonly string _connectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReglasNegocioEjecutor(IReglasNegocioBanco banco, IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _banco = banco;
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found");
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result> Ejecutar(string entidad, string operacion, object comando, int? idExcluir, CancellationToken ct, string? accion = null)
        {
            foreach (var regla in _banco.Para(entidad, operacion))
            {
                var resultado = regla.Tipo switch
                {
                    "unicidad_compuesta" => await UnicidadCompuesta(regla, comando, idExcluir),
                    "formato" => FormatoValido(regla, comando),
                    "consistencia_campos" => ConsistenciaCampos(regla, comando),
                    "no_pasado" => NoPasado(regla, comando),
                    "autorizacion_rol" => AutorizacionRol(regla, accion),
                    "proteccion_referencial" => await ProteccionReferencial(regla, idExcluir),
                    _ => Result.Ok() // tipo desconocido para este core: se ignora (forward-compat)
                };

                if (resultado.IsFailed)
                    return resultado;
            }

            return Result.Ok();
        }

        private async Task<Result> UnicidadCompuesta(ReglaNegocioData regla, object comando, int? idExcluir)
        {
            var p = regla.Perillas;
            if (string.IsNullOrEmpty(p.Tabla) || p.Campos is not { Count: > 0 })
                return Result.Fail($"Regla {regla.Id} mal generada: faltan perillas de unicidad compuesta");

            var where = new List<string>();
            var parametros = new DynamicParameters();
            for (var i = 0; i < p.Campos.Count; i++)
            {
                var valor = Leer(comando, p.Campos[i].Propiedad, regla.Id);
                if (valor.IsFailed) return valor.ToResult();
                where.Add($"[{p.Campos[i].Columna}] = @p{i}");
                parametros.Add($"p{i}", valor.Value);
            }

            var sql = $"SELECT COUNT(1) FROM [{p.Tabla}] WHERE {string.Join(" AND ", where)}";
            if (idExcluir != null)
            {
                sql += $" AND [{p.ColumnaId ?? "Id"}] <> @idExcluir";
                parametros.Add("idExcluir", idExcluir.Value);
            }

            using var connection = new SqlConnection(_connectionString);
            var repetidos = await connection.ExecuteScalarAsync<int>(sql, parametros);
            return repetidos > 0 ? Result.Fail(regla.Mensaje) : Result.Ok();
        }

        private static Result FormatoValido(ReglaNegocioData regla, object comando)
        {
            var valor = Leer(comando, regla.Perillas.Propiedad ?? "", regla.Id);
            if (valor.IsFailed) return valor.ToResult();
            if (valor.Value is not string texto || string.IsNullOrWhiteSpace(texto))
                return Result.Ok(); // vacío lo gobierna 'requerido', no 'formato'

            return FormatoCatalogo.EsValido(regla.Perillas.Formato ?? "", texto)
                ? Result.Ok()
                : Result.Fail(regla.Mensaje);
        }

        private static Result ConsistenciaCampos(ReglaNegocioData regla, object comando)
        {
            var p = regla.Perillas;
            var a = Leer(comando, p.PropiedadA ?? "", regla.Id);
            if (a.IsFailed) return a.ToResult();
            var b = Leer(comando, p.PropiedadB ?? "", regla.Id);
            if (b.IsFailed) return b.ToResult();

            if (a.Value is not IComparable ca || b.Value is null)
                return Result.Ok(); // sin ambos valores no hay qué comparar

            var cmp = ca.CompareTo(b.Value);
            var cumple = p.Operador switch
            {
                "gt" => cmp > 0,
                "gte" => cmp >= 0,
                "lt" => cmp < 0,
                "lte" => cmp <= 0,
                _ => true
            };
            return cumple ? Result.Ok() : Result.Fail(regla.Mensaje);
        }

        /// <summary>
        /// autorizacion_rol: exigeRol (quién tiene pase; vacío = nadie) y aplicaARol (a quién
        /// gobierna; vacío = a todos) contra los claims "rol" del JWT REAL — no usa
        /// ICurrentUserService a propósito: el Mock de Development daría siempre-verde y el
        /// lado negativo del test sería incomprobable. El bypass dev lleva rol "*" (pase total).
        /// </summary>
        private Result AutorizacionRol(ReglaNegocioData regla, string? accion)
        {
            var p = regla.Perillas;

            // v2: regla acotada a UNA transición — solo gobierna ESA acción.
            if (!string.IsNullOrEmpty(p.Accion) &&
                !p.Accion.Equals(accion, StringComparison.OrdinalIgnoreCase))
                return Result.Ok();

            var roles = _httpContextAccessor.HttpContext?.User?
                .FindAll("rol").Select(c => c.Value).ToList() ?? new List<string>();

            if (roles.Contains("*"))
                return Result.Ok();
            if (p.AplicaARol is { Count: > 0 } &&
                !p.AplicaARol.Any(r => roles.Contains(r, StringComparer.OrdinalIgnoreCase)))
                return Result.Ok(); // la regla no gobierna a este usuario

            var habilitado = p.ExigeRol is { Count: > 0 } &&
                p.ExigeRol.Any(r => roles.Contains(r, StringComparer.OrdinalIgnoreCase));
            return habilitado ? Result.Ok() : Result.Fail(regla.Mensaje);
        }

        /// <summary>
        /// proteccion_referencial: bloquea el eliminar si el padre tiene hijos (COUNT por FK),
        /// opcionalmente solo hijos en estadosBloqueantes. Corre en AntesDeEliminar con el Id
        /// de la entidad (idExcluir). El constraint FK de SQL sigue siendo la última trinchera:
        /// esta regla aporta la CONDICIÓN por estado y el mensaje propio.
        /// </summary>
        private async Task<Result> ProteccionReferencial(ReglaNegocioData regla, int? idPadre)
        {
            var p = regla.Perillas;
            if (idPadre is null || string.IsNullOrEmpty(p.TablaHija) || string.IsNullOrEmpty(p.FkColumna))
                return Result.Fail($"Regla {regla.Id} mal generada: faltan perillas de protección referencial");

            var sql = $"SELECT COUNT(1) FROM [{p.TablaHija}] WHERE [{p.FkColumna}] = @idPadre";
            var parametros = new DynamicParameters();
            parametros.Add("idPadre", idPadre.Value);

            if (!string.IsNullOrEmpty(p.EstadoColumna) && p.EstadosBloqueantes is { Count: > 0 })
            {
                sql += $" AND [{p.EstadoColumna}] IN @estados";
                parametros.Add("estados", p.EstadosBloqueantes);
            }

            using var connection = new SqlConnection(_connectionString);
            var hijos = await connection.ExecuteScalarAsync<int>(sql, parametros);
            return hijos > 0 ? Result.Fail(regla.Mensaje) : Result.Ok();
        }

        private static Result NoPasado(ReglaNegocioData regla, object comando)
        {
            var valor = Leer(comando, regla.Perillas.Propiedad ?? "", regla.Id);
            if (valor.IsFailed) return valor.ToResult();
            if (valor.Value is not DateTime fecha)
                return Result.Ok();

            return fecha.Date < DateTime.Today ? Result.Fail(regla.Mensaje) : Result.Ok();
        }

        private static Result<object?> Leer(object comando, string propiedad, string reglaId)
        {
            var prop = PropCache.GetOrAdd((comando.GetType(), propiedad),
                key => key.Item1.GetProperty(key.Item2, BindingFlags.Public | BindingFlags.Instance));

            if (prop == null)
                // Falla RUIDOSA: el test de plantilla la pone roja y delata al generador,
                // en vez de dejar la regla silenciosamente sin efecto.
                return Result.Fail<object?>($"Regla {reglaId} mal generada: propiedad {propiedad} no existe en {comando.GetType().Name}");

            return Result.Ok<object?>(prop.GetValue(comando));
        }
    }

    /// <summary>Catálogo de formatos conocidos (tipo 'formato' del banco).</summary>
    public static class FormatoCatalogo
    {
        private static readonly Regex Email = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        private static readonly Regex TelefonoUy = new(@"^(\+598)?\s?0?[1-9]\d{6,8}$", RegexOptions.Compiled);

        public static bool EsValido(string formato, string valor) => formato switch
        {
            "email" => Email.IsMatch(valor),
            "rut_uy" => RutUyValido(valor),
            "telefono_uy" => TelefonoUy.IsMatch(valor.Replace(" ", "").Replace("-", "")),
            "url" => Uri.TryCreate(valor, UriKind.Absolute, out var uri)
                     && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
            _ => true // formato desconocido: no bloquear (el clasificador no debería emitirlo)
        };

        /// <summary>RUT uruguayo (DGI): 12 dígitos con dígito verificador módulo 11.</summary>
        private static bool RutUyValido(string valor)
        {
            var digitos = valor.Where(char.IsDigit).ToArray();
            if (digitos.Length != 12) return false;

            int[] coeficientes = { 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            var suma = 0;
            for (var i = 0; i < 11; i++)
                suma += (digitos[i] - '0') * coeficientes[i];

            var resto = suma % 11;
            var dv = resto == 0 ? 0 : 11 - resto;
            if (dv == 10) return false;
            return dv == digitos[11] - '0';
        }
    }
}
