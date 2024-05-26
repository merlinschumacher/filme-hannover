using Microsoft.EntityFrameworkCore;

namespace kinohannover.Data
{
    public class KinohannoverContext(DbContextOptions<KinohannoverContext> options) : DbContext(options)
    {
        public DbSet<Models.Movie> Movies { get; set; } = default!;

        public DbSet<Models.Cinema> Cinema { get; set; } = default!;

        public DbSet<Models.ShowTime> ShowTime { get; set; } = default!;

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<string>().UseCollation("NOCASE");
            base.ConfigureConventions(configurationBuilder);
        }
    }
}
