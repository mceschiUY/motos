
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
            services.AddScoped<ApiMotos.Application.Agregates.Marcas.MarcaHooks>();
            services.AddScoped<ApiMotos.Domain.Agregates.Marcas.IMarcaRepositorio, ApiMotos.Infrastructure.Agregates.Marcas.Persistence.MarcaRepositorio>();
            services.AddScoped<ApiMotos.Application.Agregates.Categorias.CategoriaHooks>();
            services.AddScoped<ApiMotos.Domain.Agregates.Categorias.ICategoriaRepositorio, ApiMotos.Infrastructure.Agregates.Categorias.Persistence.CategoriaRepositorio>();
            services.AddScoped<ApiMotos.Application.Agregates.Tallas.TallaHooks>();
            services.AddScoped<ApiMotos.Domain.Agregates.Tallas.ITallaRepositorio, ApiMotos.Infrastructure.Agregates.Tallas.Persistence.TallaRepositorio>();
            services.AddScoped<ApiMotos.Application.Agregates.Colores.ColorHooks>();
            services.AddScoped<ApiMotos.Domain.Agregates.Colores.IColorRepositorio, ApiMotos.Infrastructure.Agregates.Colores.Persistence.ColorRepositorio>();
            services.AddScoped<ApiMotos.Application.Agregates.Productos.ProductoHooks>();
            services.AddScoped<ApiMotos.Domain.Agregates.Productos.IProductoRepositorio, ApiMotos.Infrastructure.Agregates.Productos.Persistence.ProductoRepositorio>();
            services.AddScoped<ApiMotos.Application.Agregates.Variantes.VarianteHooks>();
            services.AddScoped<ApiMotos.Domain.Agregates.Variantes.IVarianteRepositorio, ApiMotos.Infrastructure.Agregates.Variantes.Persistence.VarianteRepositorio>();
            services.AddScoped<ApiMotos.Application.Agregates.Depositos.DepositoHooks>();
            services.AddScoped<ApiMotos.Domain.Agregates.Depositos.IDepositoRepositorio, ApiMotos.Infrastructure.Agregates.Depositos.Persistence.DepositoRepositorio>();
            services.AddScoped<ApiMotos.Application.Agregates.MovimientosStock.MovimientoStockHooks>();
            services.AddScoped<ApiMotos.Domain.Agregates.MovimientosStock.IMovimientoStockRepositorio, ApiMotos.Infrastructure.Agregates.MovimientosStock.Persistence.MovimientoStockRepositorio>();
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
            services.AddScoped<ApiMotos.Application.Agregates.Vendedores.VendedorHooks>();
            services.AddScoped<ApiMotos.Domain.Agregates.Vendedores.IVendedorRepositorio, ApiMotos.Infrastructure.Agregates.Vendedores.Persistence.VendedorRepositorio>();
            services.AddScoped<ApiMotos.Application.Agregates.Actividades.ActividadHooks>();
            services.AddScoped<ApiMotos.Domain.Agregates.Actividades.IActividadRepositorio, ApiMotos.Infrastructure.Agregates.Actividades.Persistence.ActividadRepositorio>();
            services.AddScoped<ApiMotos.Application.Agregates.Metas.MetaHooks>();
            services.AddScoped<ApiMotos.Domain.Agregates.Metas.IMetaRepositorio, ApiMotos.Infrastructure.Agregates.Metas.Persistence.MetaRepositorio>();
            services.AddScoped<ApiMotos.Application.Agregates.Pedidos.PedidoHooks>();
            services.AddScoped<ApiMotos.Domain.Agregates.Pedidos.IPedidoRepositorio, ApiMotos.Infrastructure.Agregates.Pedidos.Persistence.PedidoRepositorio>();
            services.AddScoped<ApiMotos.Application.Agregates.PedidoLineas.PedidoLineaHooks>();
            services.AddScoped<ApiMotos.Domain.Agregates.PedidoLineas.IPedidoLineaRepositorio, ApiMotos.Infrastructure.Agregates.PedidoLineas.Persistence.PedidoLineaRepositorio>();
            // </KOSMOS:REPOS>
            return services;
        }
    }
}
