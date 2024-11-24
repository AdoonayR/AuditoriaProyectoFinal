using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditoriaQuimicos.Models
{
    public class Disposicion
    {
        public int Id { get; set; }  // Id de la disposición

        [ForeignKey("Quimico")]
        public int QuimicoId { get; set; }  // Este es el Id del Quimico asociado

        public Quimico Quimico { get; set; }  // Navegación a la entidad Quimico

        public EstadoDisposicion Estado { get; set; }
        public DateTime? FechaActualizacion { get; set; }

        public string Comentarios { get; set; }

        // Nueva propiedad para almacenar la fecha de auditoría desde Quimico
        public DateTime AuditDate { get; set; }

        public string? NoDmr { get; set; }


    }

    public enum EstadoDisposicion
    {
        Pendiente,
        EnRevision,
        FueraDelAlmacen
    }
}
