using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApiMotos.Domain.Agregates.Auditoria;

namespace ApiMotos.Infrastructure.Agregates.Auditoria.Persistence;

public class AuditLogMap : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("RT_AuditLog");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Timestamp)
            .HasColumnName("Timestamp")
            .IsRequired();

        builder.Property(e => e.UserId)
            .HasColumnName("UserId");

        builder.Property(e => e.UserName)
            .HasColumnName("UserName")
            .HasMaxLength(100);

        builder.Property(e => e.Action)
            .HasColumnName("Action")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.EntityType)
            .HasColumnName("EntityType")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.EntityId)
            .HasColumnName("EntityId")
            .HasMaxLength(50);

        builder.Property(e => e.OldValues)
            .HasColumnName("OldValues")
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.NewValues)
            .HasColumnName("NewValues")
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.IpAddress)
            .HasColumnName("IpAddress")
            .HasMaxLength(50);

        builder.Property(e => e.UserAgent)
            .HasColumnName("UserAgent")
            .HasMaxLength(500);

        builder.Property(e => e.RequestPath)
            .HasColumnName("RequestPath")
            .HasMaxLength(500);

        builder.Property(e => e.DurationMs)
            .HasColumnName("DurationMs");

        builder.Property(e => e.Success)
            .HasColumnName("Success")
            .IsRequired();

        builder.Property(e => e.ErrorMessage)
            .HasColumnName("ErrorMessage")
            .HasColumnType("nvarchar(max)");

        // Índices para búsquedas frecuentes
        builder.HasIndex(e => e.Timestamp)
            .HasDatabaseName("IX_AuditLog_Timestamp");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_AuditLog_UserId");

        builder.HasIndex(e => new { e.EntityType, e.EntityId })
            .HasDatabaseName("IX_AuditLog_EntityType_EntityId");

        builder.HasIndex(e => e.Action)
            .HasDatabaseName("IX_AuditLog_Action");
    }
}
