using System;
using System.ComponentModel.DataAnnotations;

namespace AuditoriaQuimicos.Models
{
    // Clase que representa una aprobación en el sistema
    public class Aprobacion
    {
        // Identificador único de la aprobación
        public int Id { get; set; }

        // Identificador del químico asociado a la aprobación
        public int QuimicoId { get; set; }

        // Referencia al objeto Quimico asociado a la aprobación (relación de navegación)
        public Quimico Quimico { get; set; }

        // Nombre del usuario que aprobó
        public string ApprovedBy { get; set; }

        // Fecha en la que se realizó la aprobación (puede ser nula)
        public DateTime? ApprovedDate { get; set; }

        // Tipo de aprobación (por ejemplo, "Incoming" o "Storage")
        public string ApprovalType { get; set; }
    }
}
