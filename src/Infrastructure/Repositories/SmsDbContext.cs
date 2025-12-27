using Contracts.Models;
using Contracts.Models.Cafe;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.Repositories;

public class SmsDbContext(DbContextOptions<SmsDbContext> options) : DbContext(options)
{
    public DbSet<DbResult> DbResults { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<DbResult>()
            .ToTable("DbResults")
            .HasKey(dbResult => dbResult.Id);
    }
}