using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApiMotos.Domain.Agregates.Mutation;

namespace ApiMotos.Infrastructure.Agregates.Mutation.Persistence;

public class MutacionMap : IEntityTypeConfiguration<Mutacion>
{
    public void Configure(EntityTypeBuilder<Mutacion> builder)
    {
        builder.ToTable("RT_Mutacion");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.SolicitudOriginal)
            .HasColumnName("SolicitudOriginal")
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.ResumenTecnico)
            .HasColumnName("ResumenTecnico")
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Estado)
            .HasColumnName("Estado")
            .IsRequired();

        builder.Property(e => e.FechaSolicitud)
            .HasColumnName("FechaSolicitud")
            .IsRequired();

        builder.Property(e => e.FechaAnalisis)
            .HasColumnName("FechaAnalisis");

        builder.Property(e => e.FechaEjecucion)
            .HasColumnName("FechaEjecucion");

        builder.Property(e => e.UsuarioId)
            .HasColumnName("UsuarioId")
            .IsRequired();

        builder.Property(e => e.UsuarioNombre)
            .HasColumnName("UsuarioNombre")
            .HasMaxLength(200);

        builder.Property(e => e.PreviewHtml)
            .HasColumnName("PreviewHtml")
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ArchitectureCompliance)
            .HasColumnName("ArchitectureCompliance");

        builder.Property(e => e.RiskAnalysis)
            .HasColumnName("RiskAnalysis")
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ErrorMessage)
            .HasColumnName("ErrorMessage")
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ContextoJson)
            .HasColumnName("ContextoJson")
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.RespuestaClaudeJson)
            .HasColumnName("RespuestaClaudeJson")
            .HasColumnType("nvarchar(max)");

        // Relaciones
        builder.HasMany(e => e.Impactos)
            .WithOne(i => i.Mutacion)
            .HasForeignKey(i => i.MutacionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Archivos)
            .WithOne(a => a.Mutacion)
            .HasForeignKey(a => a.MutacionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configurar backing fields para colecciones privadas (después de HasMany)
        var impactosNav = builder.Metadata.FindNavigation(nameof(Mutacion.Impactos));
        impactosNav?.SetPropertyAccessMode(PropertyAccessMode.Field);
        impactosNav?.SetField("_impactos");

        var archivosNav = builder.Metadata.FindNavigation(nameof(Mutacion.Archivos));
        archivosNav?.SetPropertyAccessMode(PropertyAccessMode.Field);
        archivosNav?.SetField("_archivos");

        // Índices
        builder.HasIndex(e => e.Estado)
            .HasDatabaseName("IX_Mutacion_Estado");

        builder.HasIndex(e => e.UsuarioId)
            .HasDatabaseName("IX_Mutacion_UsuarioId");

        builder.HasIndex(e => e.FechaSolicitud)
            .HasDatabaseName("IX_Mutacion_FechaSolicitud");
    }
}

public class MutacionImpactoMap : IEntityTypeConfiguration<MutacionImpacto>
{
    public void Configure(EntityTypeBuilder<MutacionImpacto> builder)
    {
        builder.ToTable("RT_MutacionImpacto");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.MutacionId)
            .HasColumnName("MutacionId")
            .IsRequired();

        builder.Property(e => e.Capa)
            .HasColumnName("Capa")
            .IsRequired();

        builder.Property(e => e.Tipo)
            .HasColumnName("Tipo")
            .IsRequired();

        builder.Property(e => e.Descripcion)
            .HasColumnName("Descripcion")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.RutaArchivo)
            .HasColumnName("RutaArchivo")
            .HasMaxLength(500);

        builder.Property(e => e.CodigoGenerado)
            .HasColumnName("CodigoGenerado")
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Lenguaje)
            .HasColumnName("Lenguaje")
            .HasMaxLength(50);

        builder.Property(e => e.Orden)
            .HasColumnName("Orden");

        builder.HasIndex(e => e.MutacionId)
            .HasDatabaseName("IX_MutacionImpacto_MutacionId");
    }
}

public class MutacionArchivoMap : IEntityTypeConfiguration<MutacionArchivo>
{
    public void Configure(EntityTypeBuilder<MutacionArchivo> builder)
    {
        builder.ToTable("RT_MutacionArchivo");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.MutacionId)
            .HasColumnName("MutacionId")
            .IsRequired();

        builder.Property(e => e.RutaCompleta)
            .HasColumnName("RutaCompleta")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.RutaRelativa)
            .HasColumnName("RutaRelativa")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Operacion)
            .HasColumnName("Operacion")
            .IsRequired();

        builder.Property(e => e.ContenidoOriginal)
            .HasColumnName("ContenidoOriginal")
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ContenidoNuevo)
            .HasColumnName("ContenidoNuevo")
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.HashContenido)
            .HasColumnName("HashContenido")
            .HasMaxLength(100);

        builder.Property(e => e.FechaOperacion)
            .HasColumnName("FechaOperacion")
            .IsRequired();

        builder.Property(e => e.Restaurado)
            .HasColumnName("Restaurado")
            .IsRequired();

        builder.Property(e => e.FechaRestauracion)
            .HasColumnName("FechaRestauracion");

        builder.HasIndex(e => e.MutacionId)
            .HasDatabaseName("IX_MutacionArchivo_MutacionId");
    }
}
