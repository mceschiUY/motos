// Configuraciones EF generadas (consolidado): un bloque namespace por entidad.
// Mantenido por el generador — el contenido de cada Map es el mismo que emitía {Entidad}Map.cs.

namespace ApiMotos.Infrastructure.Agregates.Clientes.Persistence
{
    using ApiMotos.Domain.Agregates.Clientes;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ClienteMap : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.ToTable("PC_CLIENTES");

            // Clave primaria
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Nombre).HasColumnName("Nombre").IsRequired().HasMaxLength(255);
            builder.Property(e => e.Telefono).HasColumnName("Telefono").HasMaxLength(255);
            builder.Property(e => e.DireccionEntrega).HasColumnName("DireccionEntrega").HasMaxLength(255);

            builder.HasIndex(e => e.Nombre).IsUnique();
        }
    }
}

namespace ApiMotos.Infrastructure.Agregates.Envios.Persistence
{
    using ApiMotos.Domain.Agregates.Envios;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class EnvioMap : IEntityTypeConfiguration<Envio>
    {
        public void Configure(EntityTypeBuilder<Envio> builder)
        {
            builder.ToTable("PC_ENVIOS");

            // Clave primaria
            builder.HasKey(e => e.Id);

            builder.Property(e => e.CodigoRastreo).HasColumnName("CodigoRastreo").IsRequired().HasMaxLength(255);
            builder.Property(e => e.Estado).HasColumnName("Estado").IsRequired().HasMaxLength(255);
            builder.Property(e => e.FechaRecibido).HasColumnName("FechaRecibido").IsRequired().HasColumnType("datetime2");
            builder.Property(e => e.FechaFactura).HasColumnName("FechaFactura").HasColumnType("datetime2");
            builder.Property(e => e.FechaEnvio).HasColumnName("FechaEnvio").HasColumnType("datetime2");
            builder.Property(e => e.FechaEntrega).HasColumnName("FechaEntrega").HasColumnType("datetime2");
            builder.Property(e => e.MotivoAnulacion).HasColumnName("MotivoAnulacion").HasMaxLength(255);
            builder.Property(e => e.ClienteId).HasColumnName("ClienteId").IsRequired();
            builder.Property(e => e.AgenciaId).HasColumnName("AgenciaId").IsRequired();

            builder.HasOne<ApiMotos.Domain.Agregates.Clientes.Cliente>().WithMany()
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_PC_ENVIOS_ClienteId");
            builder.HasOne<ApiMotos.Domain.Agregates.Agencias.Agencia>().WithMany()
                .HasForeignKey(e => e.AgenciaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_PC_ENVIOS_AgenciaId");
        }
    }
}

namespace ApiMotos.Infrastructure.Agregates.Agencias.Persistence
{
    using ApiMotos.Domain.Agregates.Agencias;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AgenciaMap : IEntityTypeConfiguration<Agencia>
    {
        public void Configure(EntityTypeBuilder<Agencia> builder)
        {
            builder.ToTable("PC_AGENCIAS");

            // Clave primaria
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Nombre).HasColumnName("Nombre").IsRequired().HasMaxLength(255);

            builder.HasIndex(e => e.Nombre).IsUnique();
        }
    }
}

namespace ApiMotos.Infrastructure.Agregates.Observaciones.Persistence
{
    using ApiMotos.Domain.Agregates.Observaciones;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ObservacionMap : IEntityTypeConfiguration<Observacion>
    {
        public void Configure(EntityTypeBuilder<Observacion> builder)
        {
            builder.ToTable("PC_OBSERVACIONES");

            // Clave primaria
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Texto).HasColumnName("Texto").IsRequired().HasMaxLength(255);
            builder.Property(e => e.FechaHora).HasColumnName("FechaHora").IsRequired().HasColumnType("datetime2");
            builder.Property(e => e.Usuario).HasColumnName("Usuario").IsRequired().HasMaxLength(255);
            builder.Property(e => e.EnvioId).HasColumnName("EnvioId").IsRequired();

            builder.HasOne<ApiMotos.Domain.Agregates.Envios.Envio>().WithMany()
                .HasForeignKey(e => e.EnvioId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_PC_OBSERVACIONES_EnvioId");
        }
    }
}

namespace ApiMotos.Infrastructure.Agregates.ParametroSLAs.Persistence
{
    using ApiMotos.Domain.Agregates.ParametroSLAs;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ParametroSLAMap : IEntityTypeConfiguration<ParametroSLA>
    {
        public void Configure(EntityTypeBuilder<ParametroSLA> builder)
        {
            builder.ToTable("PC_PARAMETROSLAS");

            // Clave primaria
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Etapa).HasColumnName("Etapa").IsRequired().HasMaxLength(255);
            builder.Property(e => e.RangoAlertaUmbralAdvertenciaDias).HasColumnName("RangoAlertaUmbralAdvertenciaDias").IsRequired();
            builder.Property(e => e.RangoAlertaLimiteDias).HasColumnName("RangoAlertaLimiteDias").IsRequired();
        }
    }
}
