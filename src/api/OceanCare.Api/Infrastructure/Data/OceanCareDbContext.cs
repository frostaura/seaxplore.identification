using Microsoft.EntityFrameworkCore;
using OceanCare.Api.Domain.Models;

namespace OceanCare.Api.Infrastructure.Data;

public class OceanCareDbContext(DbContextOptions<OceanCareDbContext> options) : DbContext(options)
{
    public DbSet<Species> Species { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<MarineAttribute> Attributes { get; set; }
    public DbSet<SpeciesAttributeValue> SpeciesAttributeValues { get; set; }
    public DbSet<SearchEmbedding> SearchEmbeddings { get; set; }
    public DbSet<Admin> Admins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Species>()
            .HasOne(s => s.Embedding)
            .WithOne(e => e.Species)
            .HasForeignKey<SearchEmbedding>(e => e.SpeciesId);

        modelBuilder.Entity<Species>()
            .HasOne(s => s.Category)
            .WithMany(c => c.Species)
            .HasForeignKey(s => s.CategoryId);

        modelBuilder.Entity<SpeciesAttributeValue>()
            .HasOne(v => v.Species)
            .WithMany(s => s.AttributeValues)
            .HasForeignKey(v => v.SpeciesId);

        modelBuilder.Entity<SpeciesAttributeValue>()
            .HasOne(v => v.Attribute)
            .WithMany(a => a.Values)
            .HasForeignKey(v => v.AttributeId);
    }
}
