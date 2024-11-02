using Microsoft.AspNetCore.Mvc;
using AuditoriaQuimicos.Models;
using AuditoriaQuimicos.Data;
using AuditoriaQuimicos.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace AuditoriaQuimicos.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailService _emailService;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            var quimicos = _context.Quimicos.ToList();
            return View(quimicos);
        }

        [HttpPost]
        [Route("api/quimicos")]
        public IActionResult SaveQuimicos([FromBody] List<Quimico> quimicos)
        {
            try
            {
                _logger.LogInformation("Iniciando el proceso de guardado de químicos.");

                var auditor = HttpContext.Session.GetString("AuditorName");
                if (string.IsNullOrEmpty(auditor))
                {
                    _logger.LogWarning("Auditor no autenticado.");
                    return Unauthorized(new { message = "Auditor no autenticado" });
                }

                if (quimicos == null || quimicos.Count == 0)
                {
                    _logger.LogWarning("Lista de químicos es nula o está vacía.");
                    return BadRequest(new { message = "Lista de químicos es nula o está vacía" });
                }

                foreach (var quimico in quimicos)
                {
                    quimico.AuditDate = DateTime.Now;
                    quimico.Auditor = auditor;
                    quimico.Comments = quimico.Comments ?? "";

                    if (!DateTime.TryParseExact(quimico.ExpirationString, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime expirationDate))
                    {
                        _logger.LogWarning("Formato de fecha de caducidad inválido.");
                        return BadRequest(new { message = "Formato de fecha de caducidad inválido" });
                    }
                    quimico.Expiration = expirationDate;

                    string commentsText = "";
                    bool hasRejectableFeature = false;

                    if (quimico.Packaging != "OK")
                    {
                        hasRejectableFeature = true;
                        commentsText += "Empaque en mal estado.\n";
                    }
                    if (quimico.Fifo != "Sí")
                    {
                        hasRejectableFeature = true;
                        commentsText += "No se está cumpliendo FIFO.\n";
                    }
                    if (quimico.Mixed != "No")
                    {
                        hasRejectableFeature = true;
                        commentsText += "Químicos mezclados.\n";
                    }
                    if (quimico.QcSeal != "Sí")
                    {
                        hasRejectableFeature = true;
                        commentsText += "No cuenta con sello de calidad.\n";
                    }
                    if (quimico.Clean != "Limpio")
                    {
                        hasRejectableFeature = true;
                        commentsText += "Limpieza del químico en mal estado.\n";
                    }

                    // Cálculo de días para caducidad y próximos a vencer
                    if (quimico.Expiration < DateTime.Now)
                    {
                        hasRejectableFeature = true;
                        var daysExpired = (int)Math.Ceiling((DateTime.Now - quimico.Expiration.Value).TotalDays);
                        commentsText += $"Químico caducado hace: {daysExpired} días.\n";
                        quimico.Result = "Rechazado";
                    }
                    else
                    {
                        var daysToExpire = (int)Math.Ceiling((quimico.Expiration.Value - DateTime.Now).TotalDays);

                        if (daysToExpire <= 30)
                        {
                            commentsText += $"Químico próximo a vencer en {daysToExpire} días.\n";
                            quimico.Result = "Próximo a vencer";
                        }
                        else
                        {
                            quimico.Result = "Aceptado";
                        }
                    }

                    if (hasRejectableFeature)
                    {
                        quimico.Result = "Rechazado";
                    }

                    quimico.Comments = commentsText;
                }

                _context.Quimicos.AddRange(quimicos);
                _context.SaveChanges();

                // Envía correo al supervisor de Incoming cuando la auditoría es completada
                _emailService.SendEmailToIncomingSupervisor();

                return Ok(new { message = "Químicos guardados exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al guardar los químicos: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al guardar los químicos." });
            }
        }

        // Método para que el supervisor de Incoming apruebe y se envíe correo al supervisor de Storage
        [HttpPost]
        public IActionResult ApproveIncoming(int quimicoId)
        {
            try
            {
                var quimico = _context.Quimicos.Find(quimicoId);
                if (quimico == null)
                {
                    _logger.LogWarning($"No se encontró el químico con ID {quimicoId}.");
                    return NotFound();
                }

                var aprobacion = _context.Aprobaciones.SingleOrDefault(a => a.QuimicoId == quimicoId);
                if (aprobacion == null)
                {
                    _logger.LogWarning($"No se encontró la aprobación para el químico con ID {quimicoId}.");
                    return NotFound();
                }

                // Aprobar por el supervisor de Incoming
                aprobacion.ApprovedByIncoming = User.Identity.Name;
                aprobacion.ApprovedDateIncoming = DateTime.Now;

                _context.SaveChanges();

                // Enviar notificación al supervisor de Storage
                _emailService.SendEmailToStorageSupervisor();

                _logger.LogInformation("Auditoría aprobada por Incoming, correo enviado a Storage.");
                return Ok(new { message = "Auditoría aprobada por Incoming, correo enviado a Storage." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error aprobando la auditoría en Incoming: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error aprobando la auditoría en Incoming." });
            }
        }
    }
}
