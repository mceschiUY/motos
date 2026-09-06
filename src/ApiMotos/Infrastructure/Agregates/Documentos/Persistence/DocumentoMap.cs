using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApiMotos.Domain.Agregates.Documentos;

namespace ApiMotos.Infrastructure.Agregates.Documentos.Persistence
{
    public class DocumentoMap : IEntityTypeConfiguration<Documento>
    {
        public void Configure(EntityTypeBuilder<Documento> builder)
        {
            builder.ToTable("PC_DOCUMENTOS");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(255);
            builder.Property(e => e.Extension).HasColumnName("extension").IsRequired().HasMaxLength(50);
            builder.Property(e => e.Contenido).HasColumnName("contenido").IsRequired();
            builder.Property(e => e.MimeType).HasColumnName("mimetype").IsRequired().HasMaxLength(100);
            builder.Property(e => e.FechaCarga).HasColumnName("fechacarga");
            builder.Property(e => e.RelacionId).HasColumnName("relacionid").IsRequired();
            builder.Property(e => e.RelacionNombre).HasColumnName("relacionnombre").IsRequired().HasMaxLength(100);
        }
    }
}
