// Configuraciones EF generadas (consolidado): un bloque namespace por entidad.
// Mantenido por el generador — el contenido de cada Map es el mismo que emitía {Entidad}Map.cs.

namespace ApiMotos.Infrastructure.Agregates.Depositos.Persistence
{
    using ApiMotos.Domain.Agregates.Depositos;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class DepositoMap : IEntityTypeConfiguration<Deposito>
    {
        public void Configure(EntityTypeBuilder<Deposito> builder)
        {
            builder.ToTable("PC_DEPOSITOS");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Codigo).HasColumnName("Codigo").IsRequired().HasMaxLength(30);
            builder.Property(e => e.Nombre).HasColumnName("Nombre").IsRequired().HasMaxLength(120);
            builder.Property(e => e.Direccion).HasColumnName("Direccion").HasMaxLength(250);
            builder.Property(e => e.Activo).HasColumnName("Activo");
            builder.HasIndex(e => e.Codigo).IsUnique();
        }
    }
}

namespace ApiMotos.Infrastructure.Agregates.MovimientosStock.Persistence
{
    using ApiMotos.Domain.Agregates.MovimientosStock;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class MovimientoStockMap : IEntityTypeConfiguration<MovimientoStock>
    {
        public void Configure(EntityTypeBuilder<MovimientoStock> builder)
        {
            builder.ToTable("PC_MOVIMIENTOS_STOCK");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.VarianteId).HasColumnName("VarianteId").IsRequired();
            builder.Property(e => e.DepositoId).HasColumnName("DepositoId").IsRequired();
            builder.Property(e => e.DepositoDestinoId).HasColumnName("DepositoDestinoId");
            builder.Property(e => e.Tipo).HasColumnName("Tipo").IsRequired().HasMaxLength(20);
            builder.Property(e => e.Cantidad).HasColumnName("Cantidad").HasColumnType("decimal(18,2)");
            builder.Property(e => e.CostoUnitario).HasColumnName("CostoUnitario").HasColumnType("decimal(18,4)");
            builder.Property(e => e.Motivo).HasColumnName("Motivo").HasMaxLength(250);
            builder.Property(e => e.DocumentoOrigen).HasColumnName("DocumentoOrigen").HasMaxLength(80);
            builder.Property(e => e.Fecha).HasColumnName("Fecha").IsRequired().HasColumnType("datetime2");
            builder.Property(e => e.Usuario).HasColumnName("Usuario").HasMaxLength(120);
            builder.HasIndex(e => e.VarianteId);
            builder.HasIndex(e => e.DepositoId);
            builder.HasOne<ApiMotos.Domain.Agregates.Variantes.Variante>().WithMany()
                .HasForeignKey(e => e.VarianteId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_PC_MOVIMIENTOS_STOCK_VarianteId");
            builder.HasOne<ApiMotos.Domain.Agregates.Depositos.Deposito>().WithMany()
                .HasForeignKey(e => e.DepositoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_PC_MOVIMIENTOS_STOCK_DepositoId");
            builder.HasOne<ApiMotos.Domain.Agregates.Depositos.Deposito>().WithMany()
                .HasForeignKey(e => e.DepositoDestinoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_PC_MOVIMIENTOS_STOCK_DepositoDestinoId");
        }
    }
}

namespace ApiMotos.Infrastructure.Agregates.Productos.Persistence
{
    using ApiMotos.Domain.Agregates.Productos;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ProductoMap : IEntityTypeConfiguration<Producto>
    {
        public void Configure(EntityTypeBuilder<Producto> builder)
        {
            builder.ToTable("PC_PRODUCTOS");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Codigo).HasColumnName("Codigo").IsRequired().HasMaxLength(60);
            builder.Property(e => e.Nombre).HasColumnName("Nombre").IsRequired().HasMaxLength(200);
            builder.Property(e => e.MarcaId).HasColumnName("MarcaId").IsRequired();
            builder.Property(e => e.CategoriaId).HasColumnName("CategoriaId").IsRequired();
            builder.Property(e => e.Descripcion).HasColumnName("Descripcion").HasMaxLength(1000);
            builder.Property(e => e.Genero).HasColumnName("Genero").IsRequired().HasMaxLength(20);
            builder.Property(e => e.Temporada).HasColumnName("Temporada").HasMaxLength(40);
            builder.Property(e => e.Material).HasColumnName("Material").HasMaxLength(120);
            builder.Property(e => e.PesoGramos).HasColumnName("PesoGramos");
            builder.Property(e => e.TipoCasco).HasColumnName("TipoCasco").HasMaxLength(20);
            builder.Property(e => e.Homologacion).HasColumnName("Homologacion").HasMaxLength(20);
            builder.Property(e => e.HomologacionVigente).HasColumnName("HomologacionVigente");
            builder.Property(e => e.FechaVencHomologacion).HasColumnName("FechaVencHomologacion").HasColumnType("date");
            builder.Property(e => e.Activo).HasColumnName("Activo");
            builder.HasIndex(e => e.Codigo).IsUnique();
            builder.HasOne<ApiMotos.Domain.Agregates.Marcas.Marca>().WithMany()
                .HasForeignKey(e => e.MarcaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_PC_PRODUCTOS_MarcaId");
            builder.HasOne<ApiMotos.Domain.Agregates.Categorias.Categoria>().WithMany()
                .HasForeignKey(e => e.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_PC_PRODUCTOS_CategoriaId");
        }
    }
}

namespace ApiMotos.Infrastructure.Agregates.Variantes.Persistence
{
    using ApiMotos.Domain.Agregates.Variantes;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class VarianteMap : IEntityTypeConfiguration<Variante>
    {
        public void Configure(EntityTypeBuilder<Variante> builder)
        {
            builder.ToTable("PC_VARIANTES");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.ProductoId).HasColumnName("ProductoId").IsRequired();
            builder.Property(e => e.TallaId).HasColumnName("TallaId");
            builder.Property(e => e.ColorId).HasColumnName("ColorId");
            builder.Property(e => e.Sku).HasColumnName("Sku").IsRequired().HasMaxLength(60);
            builder.Property(e => e.CodigoBarras).HasColumnName("CodigoBarras").HasMaxLength(40);
            builder.Property(e => e.CostoEstandar).HasColumnName("CostoEstandar").HasColumnType("decimal(18,4)");
            builder.Property(e => e.PrecioLista).HasColumnName("PrecioLista").HasColumnType("decimal(18,2)");
            builder.Property(e => e.Activo).HasColumnName("Activo");
            builder.HasIndex(e => e.Sku).IsUnique();
            builder.HasIndex(e => new { e.ProductoId, e.TallaId, e.ColorId }).IsUnique();
            builder.HasOne<ApiMotos.Domain.Agregates.Productos.Producto>().WithMany()
                .HasForeignKey(e => e.ProductoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_PC_VARIANTES_ProductoId");
            builder.HasOne<ApiMotos.Domain.Agregates.Tallas.Talla>().WithMany()
                .HasForeignKey(e => e.TallaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_PC_VARIANTES_TallaId");
            builder.HasOne<ApiMotos.Domain.Agregates.Colores.Color>().WithMany()
                .HasForeignKey(e => e.ColorId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_PC_VARIANTES_ColorId");
        }
    }
}

namespace ApiMotos.Infrastructure.Agregates.Marcas.Persistence
{
    using ApiMotos.Domain.Agregates.Marcas;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class MarcaMap : IEntityTypeConfiguration<Marca>
    {
        public void Configure(EntityTypeBuilder<Marca> builder)
        {
            builder.ToTable("PC_MARCAS");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Nombre).HasColumnName("Nombre").IsRequired().HasMaxLength(120);
            builder.Property(e => e.Pais).HasColumnName("Pais").HasMaxLength(80);
            builder.Property(e => e.Activo).HasColumnName("Activo");
            builder.HasIndex(e => e.Nombre).IsUnique();
        }
    }
}

namespace ApiMotos.Infrastructure.Agregates.Categorias.Persistence
{
    using ApiMotos.Domain.Agregates.Categorias;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CategoriaMap : IEntityTypeConfiguration<Categoria>
    {
        public void Configure(EntityTypeBuilder<Categoria> builder)
        {
            builder.ToTable("PC_CATEGORIAS");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Nombre).HasColumnName("Nombre").IsRequired().HasMaxLength(120);
            builder.Property(e => e.CategoriaPadreId).HasColumnName("CategoriaPadreId");
            builder.Property(e => e.Activo).HasColumnName("Activo");
        }
    }
}

namespace ApiMotos.Infrastructure.Agregates.Tallas.Persistence
{
    using ApiMotos.Domain.Agregates.Tallas;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TallaMap : IEntityTypeConfiguration<Talla>
    {
        public void Configure(EntityTypeBuilder<Talla> builder)
        {
            builder.ToTable("PC_TALLAS");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Nombre).HasColumnName("Nombre").IsRequired().HasMaxLength(30);
            builder.Property(e => e.Tipo).HasColumnName("Tipo").HasMaxLength(20);
            builder.Property(e => e.Orden).HasColumnName("Orden");
            builder.Property(e => e.Activo).HasColumnName("Activo");
            builder.HasIndex(e => e.Nombre).IsUnique();
        }
    }
}

namespace ApiMotos.Infrastructure.Agregates.Colores.Persistence
{
    using ApiMotos.Domain.Agregates.Colores;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ColorMap : IEntityTypeConfiguration<Color>
    {
        public void Configure(EntityTypeBuilder<Color> builder)
        {
            builder.ToTable("PC_COLORES");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Nombre).HasColumnName("Nombre").IsRequired().HasMaxLength(60);
            builder.Property(e => e.CodigoHex).HasColumnName("CodigoHex").HasMaxLength(7);
            builder.Property(e => e.Activo).HasColumnName("Activo");
            builder.HasIndex(e => e.Nombre).IsUnique();
        }
    }
}

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
