using Microsoft.EntityFrameworkCore;
using AuditoriaQuimicos.Models;

namespace AuditoriaQuimicos.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Quimico> Quimicos { get; set; }
        public DbSet<Aprobacion> Aprobaciones { get; set; }
        public DbSet<Auditor> Auditors { get; set; }
        public DbSet<Disposicion> Disposiciones { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Quimico>().ToTable("Quimicos");
        }
    }
}
