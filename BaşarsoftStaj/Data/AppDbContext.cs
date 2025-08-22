using BaşarsoftStaj.Entity;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace BaşarsoftStaj.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<PointE> PointsEF { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PointE>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Geometry)
                .HasColumnType("geometry")
                .IsRequired();
        });
    }
}
