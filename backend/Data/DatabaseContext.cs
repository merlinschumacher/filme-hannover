using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
    {
        public DbSet<Movie> Movies { get; set; } = default!;
        public DbSet<Alias> Aliases { get; set; } = default!;

        public DbSet<Cinema> Cinema { get; set; } = default!;

        public DbSet<ShowTime> ShowTime { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("NOCASE");

            modelBuilder.Entity<Cinema>()
                .OwnsOne(p => p.SchemaMetadata, b => b.ToJson());
            modelBuilder.Entity<Movie>(m => m.Property(n => n.DisplayName).UseCollation("NOCASE"));
            modelBuilder.Entity<Movie>(m => m.Navigation(n => n.Aliases).AutoInclude());
            modelBuilder.Entity<Movie>(m => m.HasMany(n => n.Aliases).WithOne(a => a.Movie).OnDelete(DeleteBehavior.Cascade));
            modelBuilder.Entity<Alias>(m => m.Property(n => n.Value).UseCollation("NOCASE"));

            base.OnModelCreating(modelBuilder);
        }
    }
}