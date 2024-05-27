using System.ComponentModel.DataAnnotations;

namespace AuditoriaQuimicos.Models
{
    public class Quimico
    {
        [Key]
        public int Id { get; set; } // Añadimos una clave primaria

        public string PartNumber { get; set; }
        public string Packaging { get; set; }
        public DateTime Expiration { get; set; }
        public string Lot { get; set; }
        public string Fifo { get; set; }
        public string Mixed { get; set; }
        public string QcSeal { get; set; }
        public string Clean { get; set; }
        public string Comments { get; set; }
        public string Result { get; set; }
    }
}
