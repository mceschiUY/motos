
using ApiMotos.Infrastructure.Agregates.Documentos.Persistence;
using ApiMotos.Domain.Agregates.Documentos;
using ApiMotos.Application.Agregates.Documentos.Commands.Cargar;
using ApiMotos.Infrastructure.Agregates.Mutation.Persistence;
using ApiMotos.Domain.Agregates.Mutation;
using ApiMotos.Application.Agregates.Mutation.Commands.AnalizarMutacion;
using ApiMotos.Infrastructure.Services.MutationEngine;
using ApiMotos.Application.Agregates.Mutation.Commands.RevertirMutacion;
using ApiMotos.Application.Agregates.Mutation.Commands.EjecutarMutacion;

// ═══════════════════════════════════════════════════════════════════════════════
// WEBAPI CORE - Registro de Módulos
// Módulos core que se incluyen en todas las APIs generadas
// ═══════════════════════════════════════════════════════════════════════════════

namespace ApiMotos.Extensions
{
    /// <summary>
    /// Extensiones para registrar módulos de servicios.
    /// Los módulos generados por Kosmos se agregan aquí automáticamente.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        // ═══════════════════════════════════════════════════════════════════════
        // MODULOS DE NEGOCIO - MARCADOR PARA GENERACIÓN AUTOMÁTICA
        // Los módulos generados por Kosmos se agregan debajo de esta línea
        // ═══════════════════════════════════════════════════════════════════════

        public static IServiceCollection AddDocumentoModule(this IServiceCollection services)
        {
            services.AddDbContext<DocumentoContext>();
            services.AddScoped<IDocumentoRepositorio, DocumentoRepositorio>();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CargarDocumentoCommand>());
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CargarDocumentoHandler).Assembly));
            return services;
        }

        /// <summary>
        /// Registra el módulo de Mutaciones ZAS
        /// </summary>
        public static IServiceCollection AddMutacionModule(this IServiceCollection services)
        {
            // DbContext
            services.AddDbContext<MutacionContext>();

            // Repositorio
            services.AddScoped<IMutacionRepositorio, MutacionRepositorio>();

            // Servicios del MutationEngine
            services.AddScoped<IMutationAnalyzerService, ClaudeAnalyzerService>();
            services.AddScoped<IMutationExecutorService, MutationExecutorService>();
            services.AddScoped<IMutationRollbackService, MutationRollbackService>();

            // MediatR handlers
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<AnalizarMutacionCommand>());

            return services;
        }


        public static IServiceCollection AddGeneratedModules(this IServiceCollection services)
        {
            services.AddDbContext<ApiMotos.Infrastructure.Generated.GeneratedContext>();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
            // <KOSMOS:REPOS>
            services.AddScoped<ApiMotos.Application.Agregates.ParametroSLAs.ParametroSLAHooks>();
            services.AddScoped<ApiMotos.Domain.Agregates.ParametroSLAs.IParametroSLARepositorio, ApiMotos.Infrastructure.Agregates.ParametroSLAs.Persistence.ParametroSLARepositorio>();
            services.AddScoped<ApiMotos.Application.Agregates.Observaciones.ObservacionHooks>();
            services.AddScoped<ApiMotos.Domain.Agregates.Observaciones.IObservacionRepositorio, ApiMotos.Infrastructure.Agregates.Observaciones.Persistence.ObservacionRepositorio>();
            services.AddScoped<ApiMotos.Application.Agregates.Agencias.AgenciaHooks>();
            services.AddScoped<ApiMotos.Domain.Agregates.Agencias.IAgenciaRepositorio, ApiMotos.Infrastructure.Agregates.Agencias.Persistence.AgenciaRepositorio>();
            services.AddScoped<ApiMotos.Application.Agregates.Envios.EnvioHooks>();
            services.AddScoped<ApiMotos.Domain.Agregates.Envios.IEnvioRepositorio, ApiMotos.Infrastructure.Agregates.Envios.Persistence.EnvioRepositorio>();
            services.AddScoped<ApiMotos.Domain.Common.IUnitOfWork, ApiMotos.Infrastructure.Generated.GeneratedUnitOfWork>();
            services.AddScoped<ApiMotos.Application.Agregates.Clientes.ClienteHooks>();
            services.AddScoped<ApiMotos.Domain.Agregates.Clientes.IClienteRepositorio, ApiMotos.Infrastructure.Agregates.Clientes.Persistence.ClienteRepositorio>();
            // </KOSMOS:REPOS>
            return services;
        }
    }
}
