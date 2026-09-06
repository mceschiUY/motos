using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApiMotos.Domain.Agregates.Evolution;

namespace ApiMotos.Infrastructure.Configurations;

public class EvolutionItemConfiguration : IEntityTypeConfiguration<EvolutionItem>
{
    public void Configure(EntityTypeBuilder<EvolutionItem> builder)
    {
        builder.ToTable("Evo_EvolutionItems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Titulo)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Descripcion)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.BusinessValue)
            .HasMaxLength(500);

        builder.Property(e => e.Estado)
            .IsRequired()
            .HasDefaultValue(EvolutionItemEstado.Backlog);

        builder.Property(e => e.Prioridad)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("medium");

        builder.Property(e => e.EntidadRelacionada)
            .HasMaxLength(100);

        builder.Property(e => e.Categoria)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.PromptMutation)
            .IsRequired();

        builder.Property(e => e.FeatureId)
            .HasMaxLength(100);

        builder.Property(e => e.Icon)
            .HasMaxLength(50)
            .HasDefaultValue("extension");

        builder.Property(e => e.Orden)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.FechaCreacion)
            .IsRequired();

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        // Índices
        builder.HasIndex(e => e.Estado);
        builder.HasIndex(e => new { e.FeatureId, e.EntidadRelacionada });
        builder.HasIndex(e => e.Prioridad);
    }
}
