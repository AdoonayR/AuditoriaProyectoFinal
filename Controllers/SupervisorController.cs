using Microsoft.AspNetCore.Mvc;
using AuditoriaQuimicos.Data;
using AuditoriaQuimicos.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace AuditoriaQuimicos.Controllers
{
    public class SupervisorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupervisorController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IActionResult IndexIncoming()
        {
            var quimicos = _context.Quimicos
                .Where(q => string.IsNullOrEmpty(q.ApprovedByIncoming))
                .AsEnumerable()
                .GroupBy(q => q.AuditDate.Date)
                .ToList();

            return View("IndexIncoming", quimicos);
        }

        public IActionResult IndexStorage()
        {
            var quimicos = _context.Quimicos
                .Where(q => !string.IsNullOrEmpty(q.ApprovedByIncoming) && string.IsNullOrEmpty(q.ApprovedByWarehouse))
                .AsEnumerable()
                .GroupBy(q => q.AuditDate.Date)
                .ToList();

            return View("IndexStorage", quimicos);
        }

        public IActionResult DetailsIncoming(string date)
        {
            if (DateTime.TryParse(date, out var auditDate))
            {
                var quimicos = _context.Quimicos
                    .Where(q => q.AuditDate.Date == auditDate && string.IsNullOrEmpty(q.ApprovedByIncoming))
                    .ToList();

                return View("Details", quimicos);
            }
            return NotFound();
        }

        public IActionResult DetailsStorage(string date)
        {
            if (DateTime.TryParse(date, out var auditDate))
            {
                var quimicos = _context.Quimicos
                    .Where(q => q.AuditDate.Date == auditDate && !string.IsNullOrEmpty(q.ApprovedByIncoming) && string.IsNullOrEmpty(q.ApprovedByWarehouse))
                    .ToList();

                return View("Details", quimicos);
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult ApproveIncoming(string date)
        {
            if (DateTime.TryParse(date, out var auditDate))
            {
                var quimicos = _context.Quimicos.Where(q => q.AuditDate.Date == auditDate && string.IsNullOrEmpty(q.ApprovedByIncoming)).ToList();
                foreach (var quimico in quimicos)
                {
                    quimico.ApprovedByIncoming = User.Identity.Name;
                }
                _context.SaveChanges();
                return Json(new { message = "Químicos aprobados exitosamente" });
            }
            return Json(new { message = "Fecha inválida" });
        }

        [HttpPost]
        public IActionResult ApproveStorage(string date)
        {
            if (DateTime.TryParse(date, out var auditDate))
            {
                var quimicos = _context.Quimicos.Where(q => q.AuditDate.Date == auditDate && !string.IsNullOrEmpty(q.ApprovedByIncoming) && string.IsNullOrEmpty(q.ApprovedByWarehouse)).ToList();
                foreach (var quimico in quimicos)
                {
                    quimico.ApprovedByWarehouse = User.Identity.Name;
                }
                _context.SaveChanges();
                return Json(new { message = "Químicos aprobados exitosamente" });
            }
            return Json(new { message = "Fecha inválida" });
        }
    }
}
