using System.ComponentModel.DataAnnotations;

namespace AuditoriaQuimicos.Models
{
    public class Quimico
    {
        public int Id { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string Packaging { get; set; } = string.Empty;
        public DateTime? Expiration { get; set; } 
        public string Lot { get; set; } = string.Empty;
        public string Fifo { get; set; } = string.Empty;
        public string Mixed { get; set; } = string.Empty;
        public string QcSeal { get; set; } = string.Empty;
        public string Clean { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string Auditor { get; set; } = string.Empty;
    }

}
