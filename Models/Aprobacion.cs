using System;
using AuditoriaQuimicos.Models;

public class Aprobacion
{
    public int Id { get; set; }

    // Relación con la tabla Quimico
    public int QuimicoId { get; set; }
    public Quimico Quimico { get; set; }

    // Columnas para las aprobaciones por diferentes supervisores
    public string ApprovedByIncoming { get; set; }
    public string ApprovedByStorage { get; set; }

    // Fechas de aprobación
    public DateTime? ApprovedDateIncoming { get; set; }
    public DateTime? ApprovedDateStorage { get; set; }
}
