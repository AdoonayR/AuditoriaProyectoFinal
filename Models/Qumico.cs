namespace AuditoriaQuimicos.Models
{
    public class Quimico
    {
        public int Id { get; set; }
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
        public string Auditor { get; set; }
        public string? ApprovedByIncoming { get; set; }
        public string? ApprovedByWarehouse { get; set; }
        public DateTime AuditDate { get; set; }
    }


}
