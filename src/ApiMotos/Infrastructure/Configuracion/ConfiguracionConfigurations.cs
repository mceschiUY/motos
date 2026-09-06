// =============================================================================
// MODULO DE CONFIGURACION - Configuraciones Entity Framework
// =============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApiMotos.Domain.Agregates.Configuracion;

namespace ApiMotos.Infrastructure.Configuracion;

/// <summary>
/// Configuracion EF para ConfiguracionSitio
/// </summary>
public class ConfiguracionSitioConfiguration : IEntityTypeConfiguration<ConfiguracionSitio>
{
    public void Configure(EntityTypeBuilder<ConfiguracionSitio> builder)
    {
        builder.ToTable("Cfg_ConfiguracionSitio");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Clave)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Valor)
            .IsRequired();

        builder.Property(c => c.Tipo)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("string");

        builder.Property(c => c.Grupo)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("general");

        builder.Property(c => c.Orden)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.Descripcion)
            .HasMaxLength(500);

        builder.Property(c => c.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.FechaCreacion)
            .IsRequired();

        // Indice unico por clave
        builder.HasIndex(c => c.Clave)
            .IsUnique();

        // Indice por grupo para busquedas
        builder.HasIndex(c => c.Grupo);

        // Indice por activo
        builder.HasIndex(c => c.Activo);
    }
}
