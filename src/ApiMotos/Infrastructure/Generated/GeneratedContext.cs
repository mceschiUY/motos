namespace ApiMotos.Infrastructure.Generated
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using ApiMotos.Infrastructure.Common;

    /// <summary>
    /// Contexto único de las entidades generadas. Mantenido por el generador vía marcadores KOSMOS.
    /// </summary>
    public class GeneratedContext : Context
    {
        // <KOSMOS:DBSETS>
        public DbSet<ApiMotos.Domain.Agregates.Marcas.Marca> Marcas { get; set; }
        public DbSet<ApiMotos.Domain.Agregates.Categorias.Categoria> Categorias { get; set; }
        public DbSet<ApiMotos.Domain.Agregates.Tallas.Talla> Tallas { get; set; }
        public DbSet<ApiMotos.Domain.Agregates.Colores.Color> Colores { get; set; }
        public DbSet<ApiMotos.Domain.Agregates.Productos.Producto> Productos { get; set; }
        public DbSet<ApiMotos.Domain.Agregates.Variantes.Variante> Variantes { get; set; }
        public DbSet<ApiMotos.Domain.Agregates.Depositos.Deposito> Depositos { get; set; }
        public DbSet<ApiMotos.Domain.Agregates.MovimientosStock.MovimientoStock> MovimientosStock { get; set; }
        public DbSet<ApiMotos.Domain.Agregates.ParametroSLAs.ParametroSLA> ParametroSLAs { get; set; }
        public DbSet<ApiMotos.Domain.Agregates.Observaciones.Observacion> Observaciones { get; set; }
        public DbSet<ApiMotos.Domain.Agregates.Agencias.Agencia> Agencias { get; set; }
        public DbSet<ApiMotos.Domain.Agregates.Envios.Envio> Envios { get; set; }
        public DbSet<ApiMotos.Domain.Agregates.Clientes.Cliente> Clientes { get; set; }
        // </KOSMOS:DBSETS>

        public GeneratedContext(IConfiguration configuration, DbContextOptions<GeneratedContext> options) : base(configuration, options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // <KOSMOS:CONFIGS>
            modelBuilder.ApplyConfiguration(new ApiMotos.Infrastructure.Agregates.Marcas.Persistence.MarcaMap());
            modelBuilder.ApplyConfiguration(new ApiMotos.Infrastructure.Agregates.Categorias.Persistence.CategoriaMap());
            modelBuilder.ApplyConfiguration(new ApiMotos.Infrastructure.Agregates.Tallas.Persistence.TallaMap());
            modelBuilder.ApplyConfiguration(new ApiMotos.Infrastructure.Agregates.Colores.Persistence.ColorMap());
            modelBuilder.ApplyConfiguration(new ApiMotos.Infrastructure.Agregates.Productos.Persistence.ProductoMap());
            modelBuilder.ApplyConfiguration(new ApiMotos.Infrastructure.Agregates.Variantes.Persistence.VarianteMap());
            modelBuilder.ApplyConfiguration(new ApiMotos.Infrastructure.Agregates.Depositos.Persistence.DepositoMap());
            modelBuilder.ApplyConfiguration(new ApiMotos.Infrastructure.Agregates.MovimientosStock.Persistence.MovimientoStockMap());
            modelBuilder.ApplyConfiguration(new ApiMotos.Infrastructure.Agregates.ParametroSLAs.Persistence.ParametroSLAMap());
            modelBuilder.ApplyConfiguration(new ApiMotos.Infrastructure.Agregates.Observaciones.Persistence.ObservacionMap());
            modelBuilder.ApplyConfiguration(new ApiMotos.Infrastructure.Agregates.Agencias.Persistence.AgenciaMap());
            modelBuilder.ApplyConfiguration(new ApiMotos.Infrastructure.Agregates.Envios.Persistence.EnvioMap());
            modelBuilder.ApplyConfiguration(new ApiMotos.Infrastructure.Agregates.Clientes.Persistence.ClienteMap());
            // </KOSMOS:CONFIGS>
        }
    }
}

namespace ApiMotos.Infrastructure.Generated
{
    using Microsoft.EntityFrameworkCore;
    using ApiMotos.Domain.Common;

    /// <summary>
    /// Adapter del puerto IUnitOfWork sobre el contexto único generado (Ola 2b):
    /// todas las escrituras del caso de uso confirman juntas o ninguna.
    /// </summary>
    public sealed class GeneratedUnitOfWork : IUnitOfWork
    {
        private readonly GeneratedContext _contexto;

        public GeneratedUnitOfWork(GeneratedContext contexto)
        {
            _contexto = contexto;
        }

        public Task<int> ConfirmarAsync(CancellationToken ct = default) =>
            _contexto.SaveChangesAsync(ct);

        public async Task EnTransaccionAsync(Func<Task> operacion, CancellationToken ct = default)
        {
            await using var tx = await _contexto.Database.BeginTransactionAsync(ct);
            await operacion();
            await _contexto.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
    }
}
