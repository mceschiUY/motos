using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApiMotos.Domain.Agregates.Reportes;

namespace ApiMotos.Infrastructure.Agregates.Reportes.Persistence;

public class ReporteHistorialMap : IEntityTypeConfiguration<ReporteHistorial>
{
    public void Configure(EntityTypeBuilder<ReporteHistorial> builder)
    {
        builder.ToTable("RT_ReporteHistorial");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.ReporteProgramadoId)
            .HasColumnName("ReporteProgramadoId");

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

        builder.Property(e => e.FechaGeneracion)
            .HasColumnName("FechaGeneracion")
            .IsRequired();

        builder.Property(e => e.GeneradoPor)
            .HasColumnName("GeneradoPor");

        builder.Property(e => e.TamanioBytes)
            .HasColumnName("TamanioBytes");

        builder.Property(e => e.Registros)
            .HasColumnName("Registros")
            .IsRequired();

        builder.Property(e => e.Estado)
            .HasColumnName("Estado")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.ErrorMensaje)
            .HasColumnName("ErrorMensaje")
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.DuracionMs)
            .HasColumnName("DuracionMs");

        builder.Property(e => e.Parametros)
            .HasColumnName("Parametros")
            .HasColumnType("nvarchar(max)");

        // Índices
        builder.HasIndex(e => e.FechaGeneracion)
            .HasDatabaseName("IX_ReporteHistorial_FechaGeneracion");

        builder.HasIndex(e => e.GeneradoPor)
            .HasDatabaseName("IX_ReporteHistorial_GeneradoPor");

        builder.HasIndex(e => e.Entidad)
            .HasDatabaseName("IX_ReporteHistorial_Entidad");

        builder.HasIndex(e => e.Estado)
            .HasDatabaseName("IX_ReporteHistorial_Estado");

        builder.HasIndex(e => e.ReporteProgramadoId)
            .HasDatabaseName("IX_ReporteHistorial_ReporteProgramadoId");
    }
}
