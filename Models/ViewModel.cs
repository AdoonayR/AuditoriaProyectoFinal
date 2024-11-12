namespace AuditoriaQuimicos.Models
{
    public class QuimicoAgrupadoViewModel
    {
        public DateTime AuditDate { get; set; }
        public string Estado { get; set; }
        public List<Quimico> Quimicos { get; set; }
    }
}
