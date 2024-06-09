using System.ComponentModel.DataAnnotations;

namespace AuditoriaQuimicos.Models
{
    public class Auditor
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string EmployeeNumber { get; set; }
    }
}
