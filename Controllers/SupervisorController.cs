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
using System.Threading.Tasks;
using AuditoriaQuimicos.Services;
using Microsoft.Extensions.Logging;
using Rotativa.AspNetCore;

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
                .OrderBy(g => g.AuditDate) // Ordenados por fecha
                .ToList();

            return View(quimicosAgrupados);
        }

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
                .OrderBy(g => g.AuditDate)
                .ToList();

            return View(quimicosAgrupados);
        }
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

            // 1. Buscar los químicos para esa fecha
            var quimicos = _context.Quimicos
                .Where(q => q.AuditDate.HasValue && q.AuditDate.Value.Date == auditDate)
                .Include(q => q.Aprobaciones)
                .ToList();

            if (!quimicos.Any())
            {
                return BadRequest("No se encontraron químicos para la fecha proporcionada.");
            }

            // 2. Actualizar aprobaciones
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

            // 3. Enviar correos según el rol
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
            else if (role == "Storage")
            {
                try
                {
                    // a) Generar el PDF en memoria usando Rotativa
                    var pdfResult = new ViewAsPdf("DetallesAuditoriaPDF", quimicos)
                    {
                        FileName = $"Auditoria_Almacen_{auditDate:yyyyMMdd}.pdf",
                        PageSize = Rotativa.AspNetCore.Options.Size.A4,
                        PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait
                    };

                    // BuildFile requiere Rotativa.AspNetCore 1.0.6+ y un ControllerContext
                    byte[] pdfBytes = await pdfResult.BuildFile(ControllerContext);

                    // b) Guardar el PDF en una carpeta local (simulación de ruta de servidor)
                    string folderPath = @"C:\Auditoriadequimicosreporte";
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string fileName = $"ReporteAuditoriMensualAlmacen_{auditDate:MMddyyyy}.pdf";
                    string fullPath = Path.Combine(folderPath, fileName);
                    System.IO.File.WriteAllBytes(fullPath, pdfBytes);

                    // c) Enviar el PDF adjunto a EHS y otros 2 departamentos
                    var destinatarios = new[] { "arodriguez7560323@cenyca.edu.mx", "dep1@dominio.com", "dep2@dominio.com" };
                    await _emailService.SendDetailsPdfAsync(pdfBytes, destinatarios,
                        $"Reporte de Auditoría Almacén{auditDate:MMddyyyy}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al enviar PDF a EHS y otros: {ex.Message}");
                }
            }

            return Json(new { message = $"Auditoria firmada correctamente por {role}" });

        }


        [HttpGet]
        public IActionResult DescargarDetallesPdf(string date)
        {
            if (string.IsNullOrEmpty(date))
            {
                return BadRequest("Se requiere una fecha/ID para generar el reporte PDF.");
            }

            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime auditDate))
            {
                return BadRequest("Fecha inválida (formato esperado: yyyy-MM-dd).");
            }

            var quimicos = _context.Quimicos
                .Where(q => q.AuditDate.HasValue && q.AuditDate.Value.Date == auditDate)
                .Include(q => q.Aprobaciones)
                .ToList();

            if (!quimicos.Any())
            {
                return BadRequest("No se encontraron químicos para esa fecha.");
            }

            return new ViewAsPdf("DetallesAuditoriaPDF", quimicos)
            {
                FileName = $"Auditoria de quimicos{auditDate:MM-dd-yyyy}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait
            };
        }

    }
}
