using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuditoriaQuimicos.Data;
using AuditoriaQuimicos.Models;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace AuditoriaQuimicos.Controllers
{
    [Authorize]
    public class SupervisorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupervisorController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Acción para la vista del Incoming Supervisor
        [Authorize(Roles = "IncomingSupervisor")]
        public IActionResult IndexIncoming()
        {
            var quimicosAgrupados = _context.Quimicos
                .Include(q => q.Aprobaciones)
                .Where(q => q.AuditDate.HasValue)
                .GroupBy(q => q.AuditDate.Value.Date)
                .Select(g => new
                {
                    AuditDate = g.Key,
                    Estado = g.All(q => q.Aprobaciones.Any(a => a.ApprovedByIncoming != null)) ? "Aprobado" : "Pendiente",
                    Quimicos = g.ToList()
                })
                .ToList();

            return View(quimicosAgrupados);
        }

        // Acción para la vista del Storage Supervisor
        [Authorize(Roles = "StorageSupervisor")]
        public IActionResult IndexStorage()
        {
            var quimicosAgrupados = _context.Quimicos
                .Include(q => q.Aprobaciones)
                .Where(q => q.AuditDate.HasValue)
                .GroupBy(q => q.AuditDate.Value.Date)
                .Select(g => new
                {
                    AuditDate = g.Key,
                    Estado = g.All(q => q.Aprobaciones.Any(a => a.ApprovedByStorage != null && a.ApprovedByIncoming != null)) ? "Aprobado" : "Pendiente",
                    Quimicos = g.ToList()
                })
                .ToList();

            return View(quimicosAgrupados);
        }

        // Método para aprobar químicos de Incoming o Storage
        [HttpPost]
        [Authorize(Roles = "IncomingSupervisor, StorageSupervisor")]
        public IActionResult Approve([FromBody] ApprovalRequest data, string role)
        {
            if (data == null || string.IsNullOrEmpty(data.Date))
            {
                return BadRequest("Fecha no proporcionada");
            }

            if (!DateTime.TryParseExact(data.Date, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime auditDate))
            {
                return BadRequest("Fecha inválida");
            }

            var quimicos = _context.Quimicos
                .Where(q => q.AuditDate.HasValue && q.AuditDate.Value.Date == auditDate)
                .Include(q => q.Aprobaciones)
                .ToList();

            if (!quimicos.Any())
            {
                return BadRequest("No se encontraron químicos para la fecha proporcionada.");
            }

            foreach (var quimico in quimicos)
            {
                var aprobacion = quimico.Aprobaciones.FirstOrDefault(a => a.QuimicoId == quimico.Id);
                if (aprobacion == null)
                {
                    aprobacion = new Aprobacion { QuimicoId = quimico.Id };
                    _context.Aprobaciones.Add(aprobacion);
                }

                // Dependiendo del rol, se realiza la aprobación
                if (role == "Incoming")
                {
                    aprobacion.ApprovedByIncoming = User.Identity.Name;
                    aprobacion.ApprovedDateIncoming = DateTime.Now;
                }
                else if (role == "Storage")
                {
                    aprobacion.ApprovedByStorage = User.Identity.Name;
                    aprobacion.ApprovedDateStorage = DateTime.Now;
                }
            }

            _context.SaveChanges();
            return Json(new { message = $"Químicos aprobados exitosamente por {role}" });
        }
    }
}
