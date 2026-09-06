namespace ApiMotos.Infrastructure.Agregates.Auditoria.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ApiMotos.Infrastructure.Common;
using ApiMotos.Domain.Agregates.Auditoria;

public class AuditLogContext : Context
{
    public DbSet<AuditLog> AuditLogs { get; set; }

    public AuditLogContext(IConfiguration configuration, DbContextOptions<AuditLogContext> options) : base(configuration, options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new AuditLogMap());
    }
}
