using AuditoriaQuimicos.Models;
using System;
using System.Collections.Generic;

namespace AuditoriaQuimicos.Models
{
    // Representa la entidad Quimico en la base de datos
    public class Quimico
    {
        // Identificador único para el químico
        public int Id { get; set; }

        // Número de parte del químico
        public string PartNumber { get; set; }

        // Estado del empaque del químico
        public string Packaging { get; set; }

        // Fecha de caducidad del químico
        public DateTime? Expiration { get; set; }

        // Lote del químico
        public string Lot { get; set; }

        // Indicador de si se sigue el principio FIFO (First In, First Out)
        public string Fifo { get; set; }

        // Indicador de si el químico está mezclado con otros
        public string Mixed { get; set; }

        // Indicador de si el químico tiene un sello de control de calidad (QC)
        public string QcSeal { get; set; }

        // Estado de limpieza del químico
        public string Clean { get; set; }

        // Comentarios adicionales sobre el químico
        public string Comments { get; set; }

        // Resultado de la auditoría del químico
        public string Result { get; set; }

        // Nombre del auditor que revisó el químico
        public string Auditor { get; set; }

        // Fecha en que se realizó la auditoría del químico
        public DateTime? AuditDate { get; set; }

        // Almacén donde se encuentra el químico
        public string Almacen { get; set; }

        // Colección de aprobaciones relacionadas con el químico
        public ICollection<Aprobacion> Aprobaciones { get; set; }
    }
}
