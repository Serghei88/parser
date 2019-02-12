using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebParser.Model;

namespace WebParser
{
    public class MixbookDbContext: DbContext
    {
        private IConfigurationRoot ConfigurationRoot { get; set; }
        
        public MixbookDbContext(IConfigurationRoot configurationRoot)
        {
            ConfigurationRoot = configurationRoot;
        }
        
        public DbSet<Project> Project { get; set; }

        public DbSet<ProjectTemplate> ProjectTemplate { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(ConfigurationRoot.GetConnectionString("DefaultConnection"));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<ProjectTemplate>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(d => d.Project);
            });
            
            modelBuilder.Entity<Page>(entity =>
            {
                entity.HasOne(d => d.Project)
                    .WithMany(p => p.Pages);
            });
        }
    }
}