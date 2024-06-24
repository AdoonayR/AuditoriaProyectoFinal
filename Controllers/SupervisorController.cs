using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuditoriaQuimicos.Data;
using AuditoriaQuimicos.Models;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace AuditoriaQuimicos.Controllers
{
    [Authorize] // Requiere que el usuario esté autorizado para acceder a este controlador
    public class SupervisorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupervisorController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Acción para mostrar la vista de supervisión de incoming
        [Authorize(Roles = "IncomingSupervisor")] // Solo accesible para usuarios con el rol de IncomingSupervisor
        public IActionResult IndexIncoming()
        {
            // Obtener los químicos y agruparlos por fecha de auditoría, incluyendo las aprobaciones
            var quimicos = _context.Quimicos
                .Include(q => q.Aprobaciones) // Incluir las aprobaciones
                .AsEnumerable()
                .GroupBy(q => q.AuditDate?.Date)
                .ToList();

            return View(quimicos);
        }

        // Acción para mostrar la vista de supervisión de storage
        [Authorize(Roles = "StorageSupervisor")] // Solo accesible para usuarios con el rol de StorageSupervisor
        public IActionResult IndexStorage()
        {
            // Obtener los químicos y agruparlos por fecha de auditoría, incluyendo las aprobaciones
            var quimicos = _context.Quimicos
                .Include(q => q.Aprobaciones) // Incluir las aprobaciones
                .AsEnumerable()
                .GroupBy(q => q.AuditDate?.Date)
                .ToList();

            return View(quimicos);
        }

        // Acción para mostrar los detalles de los químicos para incoming
        [Authorize(Roles = "IncomingSupervisor")] // Solo accesible para usuarios con el rol de IncomingSupervisor
        public IActionResult DetailsIncoming(string date)
        {
            // Intentar parsear la fecha proporcionada
            if (!DateTime.TryParseExact(date, "dd-MM-yyyy", null, DateTimeStyles.None, out DateTime auditDate))
            {
                return BadRequest("Fecha inválida");
            }

            // Obtener los químicos con la fecha de auditoría especificada
            var quimicos = _context.Quimicos
                .Where(q => q.AuditDate.HasValue && q.AuditDate.Value.Date == auditDate)
                .Include(q => q.Aprobaciones) // Incluir las aprobaciones
                .ToList();

            return View("Details", quimicos);
        }

        // Acción para mostrar los detalles de los químicos para storage
        [Authorize(Roles = "StorageSupervisor")] // Solo accesible para usuarios con el rol de StorageSupervisor
        public IActionResult DetailsStorage(string date)
        {
            // Intentar parsear la fecha proporcionada
            if (!DateTime.TryParseExact(date, "dd-MM-yyyy", null, DateTimeStyles.None, out DateTime auditDate))
            {
                return BadRequest("Fecha inválida");
            }

            // Obtener los químicos con la fecha de auditoría especificada
            var quimicos = _context.Quimicos
                .Where(q => q.AuditDate.HasValue && q.AuditDate.Value.Date == auditDate)
                .Include(q => q.Aprobaciones) // Incluir las aprobaciones
                .ToList();

            return View("Details", quimicos);
        }

        // Acción para aprobar los químicos para incoming
        [HttpPost]
        [Authorize(Roles = "IncomingSupervisor")] // Solo accesible para usuarios con el rol de IncomingSupervisor
        public IActionResult ApproveIncoming([FromBody] string date)
        {
            // Intentar parsear la fecha proporcionada
            if (!DateTime.TryParseExact(date, "dd-MM-yyyy", null, DateTimeStyles.None, out DateTime auditDate))
            {
                return BadRequest("Fecha inválida");
            }

            // Obtener los químicos que no han sido aprobados para incoming
            var quimicos = _context.Quimicos
                .Where(q => q.AuditDate.HasValue && q.AuditDate.Value.Date == auditDate &&
                            !q.Aprobaciones.Any(a => a.ApprovalType == "Incoming"))
                .ToList();

            // Aprobar cada químico
            foreach (var quimico in quimicos)
            {
                var aprobacion = new Aprobacion
                {
                    QuimicoId = quimico.Id,
                    ApprovedBy = User.Identity.Name ?? "Unknown",
                    ApprovedDate = DateTime.Now,
                    ApprovalType = "Incoming"
                };
                _context.Aprobaciones.Add(aprobacion);
            }

            // Guardar cambios en la base de datos
            _context.SaveChanges();
            return Json(new { message = "Químicos aprobados exitosamente" });
        }

        // Acción para aprobar los químicos para storage
        [HttpPost]
        [Authorize(Roles = "StorageSupervisor")] // Solo accesible para usuarios con el rol de StorageSupervisor
        public IActionResult ApproveStorage([FromBody] string date)
        {
            // Intentar parsear la fecha proporcionada
            if (!DateTime.TryParseExact(date, "dd-MM-yyyy", null, DateTimeStyles.None, out DateTime auditDate))
            {
                return BadRequest("Fecha inválida");
            }

            // Obtener los químicos que han sido aprobados para incoming pero no para storage
            var quimicos = _context.Quimicos
                .Where(q => q.AuditDate.HasValue && q.AuditDate.Value.Date == auditDate &&
                            q.Aprobaciones.Any(a => a.ApprovalType == "Incoming") &&
                            !q.Aprobaciones.Any(a => a.ApprovalType == "Storage"))
                .ToList();

            // Aprobar cada químico
            foreach (var quimico in quimicos)
            {
                var aprobacion = new Aprobacion
                {
                    QuimicoId = quimico.Id,
                    ApprovedBy = User.Identity.Name ?? "Unknown",
                    ApprovedDate = DateTime.Now,
                    ApprovalType = "Storage"
                };
                _context.Aprobaciones.Add(aprobacion);
            }

            // Guardar cambios en la base de datos
            _context.SaveChanges();
            return Json(new { message = "Químicos aprobados exitosamente" });
        }
    }
}
