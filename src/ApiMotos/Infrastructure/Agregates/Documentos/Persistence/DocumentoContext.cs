namespace ApiMotos.Infrastructure.Agregates.Documentos.Persistence
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using ApiMotos.Infrastructure.Common;
    using ApiMotos.Domain.Agregates.Documentos;

    public class DocumentoContext : Context
    {
        public DbSet<Documento> Documentos { get; set; }

        public DocumentoContext(IConfiguration configuration, DbContextOptions<DocumentoContext> options) : base(configuration, options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new DocumentoMap());
        }
    }
}
