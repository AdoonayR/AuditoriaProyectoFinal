using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuditoriaQuimicos.Data;
using AuditoriaQuimicos.Models;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AuditoriaQuimicos.Controllers
{
    [Authorize]
    public class SupervisorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<SupervisorController> _logger;


        public SupervisorController(ApplicationDbContext context, IEmailService emailService, ILogger<SupervisorController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }



        // Acción para la vista del Incoming Supervisor
        [Authorize(Roles = "IncomingSupervisor")]
        public IActionResult IndexIncoming()
        {
            var quimicosAgrupados = _context.Quimicos
                .Include(q => q.Aprobaciones)
                .Where(q => q.AuditDate.HasValue)
                .GroupBy(q => q.AuditDate.Value.Date)
                .Select(g => new QuimicoAgrupadoViewModel
                {
                    AuditDate = g.Key,
                    Estado = g.All(q => q.Aprobaciones.Any(a => a.ApprovedByIncoming != null)) ? "Aprobado" : "Pendiente",
                    Quimicos = g.ToList()
                })
                .OrderBy(g => g.AuditDate) // Ordenamos por la fecha de auditoría
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
                .Select(g => new QuimicoAgrupadoViewModel
                {
                    AuditDate = g.Key,
                    Estado = g.All(q => q.Aprobaciones.Any(a => a.ApprovedByStorage != null && a.ApprovedByIncoming != null)) ? "Aprobado" : "Pendiente",
                    Quimicos = g.ToList()
                })
                .OrderBy(g => g.AuditDate) // Ordenamos por la fecha de auditoría
                .ToList();

            return View(quimicosAgrupados);
        }

        // Método para aprobar químicos de Incoming o Storage
        [HttpPost]
        [Authorize(Roles = "IncomingSupervisor, StorageSupervisor")]
        public async Task<IActionResult> Approve([FromBody] ApprovalRequest data, string role)
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

            var aprobacionesPendientes = new List<Aprobacion>();

            foreach (var quimico in quimicos)
            {
                var aprobacion = quimico.Aprobaciones.FirstOrDefault(a => a.QuimicoId == quimico.Id)
                                 ?? new Aprobacion { QuimicoId = quimico.Id };

                if (role == "Incoming")
                {
                    aprobacion.ApprovedByIncoming = User.Identity.Name;
                    aprobacion.ApprovedDateIncoming = DateTime.Now;
                }
                else if (role == "Storage" && aprobacion.ApprovedByIncoming != null)
                {
                    aprobacion.ApprovedByStorage = User.Identity.Name;
                    aprobacion.ApprovedDateStorage = DateTime.Now;
                }

                aprobacionesPendientes.Add(aprobacion);
            }

            _context.Aprobaciones.UpdateRange(aprobacionesPendientes);
            await _context.SaveChangesAsync();

            if (role == "Incoming")
            {
                try
                {
                    await _emailService.SendEmailToStorageSupervisorAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al enviar correo al Storage Supervisor: {ex.Message}");
                }
            }

            return Json(new { message = $"Auditoria firmada correctamente por {role}" });
        }


    }
}
