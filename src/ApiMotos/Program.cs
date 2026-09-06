using ApiMotos.Infrastructure.Common;
using ApiMotos.Extensions;
using ApiMotos.Middleware;
using ApiMotos.Application.Common.Behaviors;
using ApiMotos.Shared.Application.Behaviors;
using ApiMotos.Shared.Abstractions;
using ApiMotos.Infrastructure.Services;
using ApiMotos.Domain.Agregates.Seguridad;
using ApiMotos.Infrastructure.Agregates.Seguridad.Persistence;
using ApiMotos.Domain.Agregates.Configuracion;
using ApiMotos.Infrastructure.Agregates.Configuracion.Persistence;
using ApiMotos.Domain.Repositories;
using ApiMotos.Infrastructure.Repositories;
using ApiMotos.Domain.Agregates.Auditoria;
using ApiMotos.Infrastructure.Agregates.Auditoria.Persistence;
using ApiMotos.Domain.Agregates.Reportes;
using ApiMotos.Infrastructure.Agregates.Reportes.Persistence;
using ApiMotos.Application.Agregates.Auth.Queries.Login;
using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DbContext base
builder.Services.AddDbContext<Context>();
builder.Services.AddGeneratedModules();

// Modulo Documento
builder.Services.AddDocumentoModule();

// Modulo Mutacion (ZAS - Mutation Engine)
builder.Services.AddMutacionModule();

// ═══════════════════════════════════════════════════════════════════════════════
// MODULOS CORE
// ═══════════════════════════════════════════════════════════════════════════════

// Modulo Seguridad - Repositorios
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<ICapabilityRepositorio, CapabilityRepositorio>();
builder.Services.AddScoped<IRolRepositorio, RolRepositorio>();
builder.Services.AddScoped<IPerfilRepositorio, PerfilRepositorio>();

// Modulo Configuracion - Repositorio
builder.Services.AddScoped<IConfiguracionRepositorio, ConfiguracionRepositorio>();

// Modulo Evolution - Repositorio
builder.Services.AddScoped<IEvolutionItemRepositorio, EvolutionItemRepositorio>();

// Modulo Auditoria - Context y Repositorio
builder.Services.AddDbContext<AuditLogContext>();
builder.Services.AddScoped<IAuditLogRepositorio, AuditLogRepositorio>();

// Modulo Reportes - Context, Repositorios y Servicios
builder.Services.AddDbContext<ReportesContext>();
builder.Services.AddScoped<IReporteProgramadoRepositorio, ReporteProgramadoRepositorio>();
builder.Services.AddScoped<IReporteHistorialRepositorio, ReporteHistorialRepositorio>();
builder.Services.AddScoped<IExportService, ExportService>();

// Banco de Reglas (reglas-dato): banco singleton (carga reglas-negocio.json) + ejecutor
builder.Services.AddSingleton<ApiMotos.Application.Common.Generated.IReglasNegocioBanco,
    ApiMotos.Application.Common.Generated.ReglasNegocioBanco>();
builder.Services.AddSingleton<ApiMotos.Application.Common.Generated.IReglasNegocioEjecutor,
    ApiMotos.Application.Common.Generated.ReglasNegocioEjecutor>();

// Ciclos de vida como datos: banco singleton (carga ciclos-vida.json) — la matriz de
// transiciones la interpreta GenericTransicionHandler, ya no se estampa por transición.
builder.Services.AddSingleton<ApiMotos.Application.Common.Generated.ICicloBanco,
    ApiMotos.Application.Common.Generated.CicloBanco>();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<LoginQuery>());

// ═══════════════════════════════════════════════════════════════════════════════
// AUTENTICACION JWT
// ═══════════════════════════════════════════════════════════════════════════════

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "DefaultSecretKey1234567890!@#$%^&*()";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "ApiMotos",
        ValidAudience = jwtSettings["Audience"] ?? "SiteMotos",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE SEGURIDAD Y VALIDACION UNIVERSAL
// ═══════════════════════════════════════════════════════════════════════════════

// HttpContextAccessor (necesario para CurrentUserService)
builder.Services.AddHttpContextAccessor();

// Reloj inyectable (puerto IClock, Refactorización C.1): las reglas temporales generadas
// leen Clock.Current — acá el composition root lo fija al reloj de sistema. En tests de
// dominio se pisa Clock.Current con un reloj fijo.
var relojSistema = new ApiMotos.Infrastructure.Services.SystemClock();
builder.Services.AddSingleton<ApiMotos.Domain.Common.IClock>(relojSistema);
ApiMotos.Domain.Common.Clock.Current = relojSistema;

// Puerto de lecturas (IQueryService, Refactorización Ola 2): los query handlers piden la
// ejecución por acá — el único que abre SqlConnection es el adapter de Infrastructure.
builder.Services.AddScoped<ApiMotos.Application.Common.Abstractions.IQueryService,
    ApiMotos.Infrastructure.Services.DapperQueryService>();

// Puerto de eventos de dominio (IEventPublisher, Refactorización Ola 3): las cascadas
// van por evento — adapter in-process sobre MediatR (outbox recién si hay integración externa).
builder.Services.AddScoped<ApiMotos.Domain.Common.IEventPublisher,
    ApiMotos.Infrastructure.Services.MediatorEventPublisher>();

// Asistente de voz (puerto en Application, adaptador OpenAI en Infrastructure).
// Sin OPENAI_API_KEY / Asistente:OpenAIApiKey → Habilitado=false y el front ni muestra el botón.
builder.Services.AddScoped<ApiMotos.Application.Agregates.Asistente.IAsistenteVozService,
    ApiMotos.Infrastructure.Services.OpenAIRealtimeService>();

// Current User Service - Lee claims JWT del usuario autenticado
// Mock (TODAS las capabilities) SOLO en build Debug Y entorno Development: un binario
// Debug corriendo como Production usa el servicio real — el bypass no viaja a clientes.
#if DEBUG
if (builder.Environment.IsDevelopment())
    builder.Services.AddScoped<ICurrentUserService, MockCurrentUserService>();
else
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
#else
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
#endif

// Pipeline Behaviors (orden importa: Security -> FluentValidation -> AggregateValidation -> Audit)
// 1. Security se ejecuta PRIMERO - fail-fast si no tiene permisos
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SecurityBehavior<,>));
// 2. FluentValidation se ejecuta SEGUNDO para validar formato/sintaxis
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(FluentValidationBehavior<,>));
// 3. AggregateValidation se ejecuta TERCERO para validar reglas de negocio
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AggregateValidationBehavior<,>));
// 4. Audit se ejecuta CUARTO para registrar la acción (después de validaciones, antes del handler)
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));

// ═══════════════════════════════════════════════════════════════════════════════
// CONFIGURACION GENERAL
// ═══════════════════════════════════════════════════════════════════════════════

// HttpClient for external APIs
builder.Services.AddHttpClient();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS
app.UseCors("AllowAll");

// NO usar HTTPS redirection en desarrollo para evitar problemas de CORS
// En producción nginx/docker maneja HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Middleware de validacion de negocio (Sistema de Validacion Universal)
// Captura BusinessValidationException y las mapea a HTTP 422
app.UseBusinessValidation();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Auto-provisión de base de datos (inyectado por Forja, ver DbBootstrap.cs)
app.EnsureDatabaseProvisioned();

app.Run();
