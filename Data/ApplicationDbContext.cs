using Microsoft.EntityFrameworkCore;
using AuditoriaQuimicos.Models;

namespace AuditoriaQuimicos.Data
{
    // Clase que representa el contexto de la base de datos para la aplicación
    public class ApplicationDbContext : DbContext
    {
        // Constructor que acepta opciones de configuración para el contexto
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet que representa la tabla de químicos en la base de datos
        public DbSet<Quimico> Quimicos { get; set; }

        // DbSet que representa la tabla de aprobaciones en la base de datos
        public DbSet<Aprobacion> Aprobaciones { get; set; }

        // DbSet que representa la tabla de auditores en la base de datos
        public DbSet<Auditor> Auditors { get; set; }
    }
}
