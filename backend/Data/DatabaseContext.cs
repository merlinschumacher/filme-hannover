using backend.Models;
using backend.Data.Converters;
using Microsoft.EntityFrameworkCore;
using Schema.NET;
using Movie = backend.Models.Movie;

namespace backend.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<Movie> Movies { get; set; } = default!;
        public DbSet<Alias> Aliases { get; set; } = default!;
        public DbSet<Cinema> Cinema { get; set; } = default!;
        public DbSet<ShowTime> ShowTime { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("NOCASE");

            modelBuilder.Entity<Cinema>()
                .Property(c => c.Address)
                .HasConversion(new PostalAddressConverter()); // Use the custom converter

            modelBuilder.Entity<Movie>(m => m.Property(n => n.DisplayName).UseCollation("NOCASE"));
            modelBuilder.Entity<Movie>(m => m.Navigation(n => n.Aliases).AutoInclude());
            modelBuilder.Entity<Movie>(m => m.HasMany(n => n.Aliases).WithOne(a => a.Movie).OnDelete(DeleteBehavior.Cascade));
            modelBuilder.Entity<Alias>(m => m.Property(n => n.Value).UseCollation("NOCASE"));

            base.OnModelCreating(modelBuilder);
        }
    }
}