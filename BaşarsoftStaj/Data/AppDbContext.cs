using BaşarsoftStaj.Entity;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace BaşarsoftStaj.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Shape> Shapes { get; set; }
    public DbSet<Rule> Rules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<Shape>(entity =>
        {
            entity.ToTable("Shapes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Geometry)
                .IsRequired()
                .HasColumnType("geometry");
        });

        modelBuilder.Entity<Rule>().ToTable("Rules");
    }
}
