using Microsoft.EntityFrameworkCore;

namespace kinohannover.Data
{
    public class KinohannoverContext(DbContextOptions<KinohannoverContext> options) : DbContext(options)
    {
        public DbSet<Models.Movie> Movies { get; set; } = default!;
        public DbSet<Models.Alias> Aliases { get; set; } = default!;

        public DbSet<Models.Cinema> Cinema { get; set; } = default!;

        public DbSet<Models.ShowTime> ShowTime { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("NOCASE");

            modelBuilder.Entity<Models.Movie>(m => m.Property(n => n.DisplayName).UseCollation("NOCASE"));
            modelBuilder.Entity<Models.Movie>(m => m.Navigation(n => n.Aliases).AutoInclude());
            modelBuilder.Entity<Models.Movie>(m => m.HasMany(n => n.Aliases).WithOne(a => a.Movie).OnDelete(DeleteBehavior.Cascade));
            modelBuilder.Entity<Models.Alias>(m => m.Property(n => n.Value).UseCollation("NOCASE"));

            base.OnModelCreating(modelBuilder);
        }
    }
}