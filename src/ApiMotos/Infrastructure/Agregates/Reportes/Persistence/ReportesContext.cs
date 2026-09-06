namespace ApiMotos.Infrastructure.Agregates.Reportes.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ApiMotos.Infrastructure.Common;
using ApiMotos.Domain.Agregates.Reportes;

public class ReportesContext : Context
{
    public DbSet<ReporteProgramado> ReportesProgramados { get; set; }
    public DbSet<ReporteHistorial> ReportesHistorial { get; set; }

    public ReportesContext(IConfiguration configuration, DbContextOptions<ReportesContext> options) : base(configuration, options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ReporteProgramadoMap());
        modelBuilder.ApplyConfiguration(new ReporteHistorialMap());
    }
}
