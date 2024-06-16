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
        public DbSet<Auditor> Auditors { get; set; }
    }
}
