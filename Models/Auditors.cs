using System.ComponentModel.DataAnnotations;

namespace AuditoriaQuimicos.Models
{
    // Clase que representa a un auditor en el sistema
    public class Auditor
    {
        // Identificador único del auditor
        [Key]
        public int Id { get; set; }

        // Nombre del auditor
        [Required]
        public string Name { get; set; }

        // Número de empleado del auditor, debe ser único y es requerido
        [Required]
        public string EmployeeNumber { get; set; }

        // Rol del auditor, por ejemplo, "IncomingSupervisor", "StorageSupervisor" o "Auditor"
        [Required]
        public string Role { get; set; }
    }
}
