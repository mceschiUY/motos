namespace ApiMotos.Infrastructure.Agregates.Mutation.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ApiMotos.Infrastructure.Common;
using ApiMotos.Domain.Agregates.Mutation;

public class MutacionContext : Context
{
    public DbSet<Mutacion> Mutaciones { get; set; }
    public DbSet<MutacionImpacto> MutacionImpactos { get; set; }
    public DbSet<MutacionArchivo> MutacionArchivos { get; set; }

    public MutacionContext(IConfiguration configuration, DbContextOptions<MutacionContext> options)
        : base(configuration, options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new MutacionMap());
        modelBuilder.ApplyConfiguration(new MutacionImpactoMap());
        modelBuilder.ApplyConfiguration(new MutacionArchivoMap());
    }
}
