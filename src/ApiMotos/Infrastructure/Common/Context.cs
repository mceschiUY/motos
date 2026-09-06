using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ApiMotos.Domain.Agregates.Seguridad;
using ApiMotos.Domain.Agregates.Configuracion;
using ApiMotos.Domain.Agregates.Evolution;

namespace ApiMotos.Infrastructure.Common
{
    /// <summary>
    /// DbContext base de la aplicación.
    /// Los DbSets de entidades se agregarán cuando se generen con Kosmos.
    /// </summary>
    public class Context : DbContext
    {
        protected readonly IConfiguration _configuration;

        public Context(IConfiguration configuration, DbContextOptions<Context> options) : base(options)
        {
            _configuration = configuration;
        }

        // Constructor protegido para clases derivadas
        protected Context(IConfiguration configuration, DbContextOptions options) : base(options)
        {
            _configuration = configuration;
        }

        // ═══════════════════════════════════════════════════════════════════════
        // MÓDULO DE SEGURIDAD
        // ═══════════════════════════════════════════════════════════════════════
        public DbSet<Capability> Capabilities { get; set; } = null!;
        public DbSet<Rol> Roles { get; set; } = null!;
        public DbSet<RolCapability> RolCapabilities { get; set; } = null!;
        public DbSet<Perfil> Perfiles { get; set; } = null!;
        public DbSet<PerfilRol> PerfilRoles { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; } = null!;

        // ═══════════════════════════════════════════════════════════════════════
        // MÓDULO DE CONFIGURACIÓN
        // ═══════════════════════════════════════════════════════════════════════
        public DbSet<ConfiguracionSitio> ConfiguracionesSitio { get; set; } = null!;

        // ═══════════════════════════════════════════════════════════════════════
        // MÓDULO DE EVOLUTION
        // ═══════════════════════════════════════════════════════════════════════
        public DbSet<EvolutionItem> EvolutionItems { get; set; } = null!;

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

            // Aplicar configuraciones de Seguridad
            modelBuilder.ApplyConfiguration(new Infrastructure.Seguridad.CapabilityConfiguration());
            modelBuilder.ApplyConfiguration(new Infrastructure.Seguridad.RolConfiguration());
            modelBuilder.ApplyConfiguration(new Infrastructure.Seguridad.RolCapabilityConfiguration());
            modelBuilder.ApplyConfiguration(new Infrastructure.Seguridad.PerfilConfiguration());
            modelBuilder.ApplyConfiguration(new Infrastructure.Seguridad.PerfilRolConfiguration());
            modelBuilder.ApplyConfiguration(new Infrastructure.Seguridad.UsuarioConfiguration());

            // Aplicar configuraciones de Configuracion
            modelBuilder.ApplyConfiguration(new Infrastructure.Configuracion.ConfiguracionSitioConfiguration());

            // Aplicar configuraciones de Evolution
            modelBuilder.ApplyConfiguration(new Infrastructure.Configurations.EvolutionItemConfiguration());
        }
    }
}
