using Microsoft.EntityFrameworkCore;

namespace kinohannover.Data
{
    public class KinohannoverContext(DbContextOptions<KinohannoverContext> options) : DbContext(options)
    {
        public DbSet<Models.Movie> Movies { get; set; } = default!;

        public DbSet<Models.Cinema> Cinema { get; set; } = default!;

        public DbSet<Models.ShowTime> ShowTime { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Movie>().HasMany(m => m.ShowTimes).WithOne(s => s.Movie).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Models.Cinema>().HasMany(c => c.Movies).WithMany(m => m.Cinemas);
        }
    }
}
