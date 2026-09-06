// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE SEGURIDAD ZAS - Configuraciones Entity Framework
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApiMotos.Domain.Agregates.Seguridad;

namespace ApiMotos.Infrastructure.Seguridad;

/// <summary>
/// Configuración EF para Capability
/// </summary>
public class CapabilityConfiguration : IEntityTypeConfiguration<Capability>
{
    public void Configure(EntityTypeBuilder<Capability> builder)
    {
        builder.ToTable("Seg_Capabilities");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Descripcion)
            .HasMaxLength(500);

        builder.Property(c => c.Modulo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        // Índice único por nombre
        builder.HasIndex(c => c.Nombre)
            .IsUnique();

        // Índice por módulo para búsquedas
        builder.HasIndex(c => c.Modulo);
    }
}

/// <summary>
/// Configuración EF para Rol
/// </summary>
public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Seg_Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Descripcion)
            .HasMaxLength(500);

        builder.Property(r => r.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.FechaCreacion)
            .IsRequired();

        // Índice único por nombre
        builder.HasIndex(r => r.Nombre)
            .IsUnique();
    }
}

/// <summary>
/// Configuración EF para RolCapability (tabla intermedia)
/// </summary>
public class RolCapabilityConfiguration : IEntityTypeConfiguration<RolCapability>
{
    public void Configure(EntityTypeBuilder<RolCapability> builder)
    {
        builder.ToTable("Seg_RolCapabilities");

        // Clave compuesta
        builder.HasKey(rc => new { rc.RolId, rc.CapabilityId });

        builder.Property(rc => rc.FechaAsignacion)
            .IsRequired();

        // Relación con Rol
        builder.HasOne(rc => rc.Rol)
            .WithMany(r => r.RolCapabilities)
            .HasForeignKey(rc => rc.RolId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Capability
        builder.HasOne(rc => rc.Capability)
            .WithMany(c => c.RolCapabilities)
            .HasForeignKey(rc => rc.CapabilityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Configuración EF para Perfil
/// </summary>
public class PerfilConfiguration : IEntityTypeConfiguration<Perfil>
{
    public void Configure(EntityTypeBuilder<Perfil> builder)
    {
        builder.ToTable("Seg_Perfiles");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Descripcion)
            .HasMaxLength(500);

        builder.Property(p => p.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.FechaCreacion)
            .IsRequired();

        // Índice único por nombre
        builder.HasIndex(p => p.Nombre)
            .IsUnique();
    }
}

/// <summary>
/// Configuración EF para PerfilRol (tabla intermedia)
/// </summary>
public class PerfilRolConfiguration : IEntityTypeConfiguration<PerfilRol>
{
    public void Configure(EntityTypeBuilder<PerfilRol> builder)
    {
        builder.ToTable("Seg_PerfilRoles");

        // Clave compuesta
        builder.HasKey(pr => new { pr.PerfilId, pr.RolId });

        builder.Property(pr => pr.FechaAsignacion)
            .IsRequired();

        // Relación con Perfil
        builder.HasOne(pr => pr.Perfil)
            .WithMany(p => p.PerfilRoles)
            .HasForeignKey(pr => pr.PerfilId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Rol
        builder.HasOne(pr => pr.Rol)
            .WithMany(r => r.PerfilRoles)
            .HasForeignKey(pr => pr.RolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Configuración EF para Usuario
/// </summary>
public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Seg_Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.NombreCompleto)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.FechaCreacion)
            .IsRequired();

        builder.Property(u => u.IntentosFallidos)
            .IsRequired()
            .HasDefaultValue(0);

        // Índice único por UserName
        builder.HasIndex(u => u.UserName)
            .IsUnique();

        // Índice único por Email
        builder.HasIndex(u => u.Email)
            .IsUnique();

        // Índice por PerfilId para búsquedas
        builder.HasIndex(u => u.PerfilId);

        // Relación con Perfil
        builder.HasOne(u => u.Perfil)
            .WithMany(p => p.Usuarios)
            .HasForeignKey(u => u.PerfilId)
            .OnDelete(DeleteBehavior.Restrict); // No eliminar perfil si tiene usuarios
    }
}
