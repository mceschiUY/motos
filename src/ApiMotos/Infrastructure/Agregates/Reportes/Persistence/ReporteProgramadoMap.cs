using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApiMotos.Domain.Agregates.Reportes;

namespace ApiMotos.Infrastructure.Agregates.Reportes.Persistence;

public class ReporteProgramadoMap : IEntityTypeConfiguration<ReporteProgramado>
{
    public void Configure(EntityTypeBuilder<ReporteProgramado> builder)
    {
        builder.ToTable("RT_ReporteProgramado");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Nombre)
            .HasColumnName("Nombre")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.TipoReporte)
            .HasColumnName("TipoReporte")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Entidad)
            .HasColumnName("Entidad")
            .HasMaxLength(100);

        builder.Property(e => e.Formato)
            .HasColumnName("Formato")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.CronExpression)
            .HasColumnName("CronExpression")
            .HasMaxLength(100);

        builder.Property(e => e.Destinatarios)
            .HasColumnName("Destinatarios")
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Parametros)
            .HasColumnName("Parametros")
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Columnas)
            .HasColumnName("Columnas")
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Activo)
            .HasColumnName("Activo")
            .IsRequired();

        builder.Property(e => e.UltimaEjecucion)
            .HasColumnName("UltimaEjecucion");

        builder.Property(e => e.ProximaEjecucion)
            .HasColumnName("ProximaEjecucion");

        builder.Property(e => e.CreadoPor)
            .HasColumnName("CreadoPor")
            .IsRequired();

        builder.Property(e => e.FechaCreacion)
            .HasColumnName("FechaCreacion")
            .IsRequired();

        // Índices
        builder.HasIndex(e => e.Activo)
            .HasDatabaseName("IX_ReporteProgramado_Activo");

        builder.HasIndex(e => e.Entidad)
            .HasDatabaseName("IX_ReporteProgramado_Entidad");

        builder.HasIndex(e => e.CreadoPor)
            .HasDatabaseName("IX_ReporteProgramado_CreadoPor");

        builder.HasIndex(e => e.ProximaEjecucion)
            .HasDatabaseName("IX_ReporteProgramado_ProximaEjecucion");
    }
}
