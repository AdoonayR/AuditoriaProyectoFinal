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
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        public IActionResult IndexPrioridades()
        {
            try
            {
                // Obtenemos los químicos próximos a vencer que no estén rechazados
                var quimicosProximos = _context.Disposiciones
                    .Include(d => d.Quimico) // Incluir la relación con Quimico
                    .AsEnumerable() // Cambiar a evaluación en memoria
                    .Where(d => d.Quimico != null &&
                                d.Quimico.Expiration.HasValue &&
                                (d.Quimico.Expiration.Value - DateTime.Now).Days <= 30 && // Próximos a vencer
                                d.Estado == EstadoDisposicion.EnRevision) // Filtramos solo en revisión (no rechazados)
                    .ToList();

                // Retornamos la vista con el modelo
                return View(quimicosProximos);
            }
            catch (Exception ex)
            {
                // Logueamos el error y mostramos una página de error personalizada
                _logger.LogError($"Error en IndexPrioridades: {ex.Message}");
                return View("Error", new ErrorViewModel { Message = "Hubo un problema al cargar los químicos próximos a vencer." });
            }
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
                    quimico.AuditDate = quimico.AuditDate ?? DateTime.Now;
                    quimico.Auditor = auditor;
                    quimico.Comments = quimico.Comments ?? "";

                    if (!DateTime.TryParseExact(quimico.ExpirationString, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime expirationDate))
                    {
                        _logger.LogWarning("Formato de fecha de caducidad inválido.");
                        return BadRequest(new { message = "Formato de fecha de caducidad inválido" });
                    }
                    quimico.Expiration = expirationDate;

                    bool isRejected = false;
                    string commentsText = ValidarQuimico(quimico, ref isRejected);

                    quimico.Comments = commentsText;
                    _context.Quimicos.Add(quimico);
                }

                // Guardamos los cambios para obtener los IDs generados para cada químico
                _context.SaveChanges();

                // Crear disposiciones solo para los químicos rechazados o próximos a vencer
                foreach (var quimico in quimicos)
                {
                    if (quimico.Result == "Rechazado" || quimico.Result == "Próximo a vencer")
                    {
                        var nuevaDisposicion = new Disposicion
                        {
                            QuimicoId = quimico.Id,
                            Estado = quimico.Result == "Rechazado" ? EstadoDisposicion.Pendiente : EstadoDisposicion.EnRevision,
                            FechaActualizacion = DateTime.Now,
                            Comentarios = quimico.Comments,
                            AuditDate = quimico.AuditDate ?? DateTime.Now
                        };
                        _context.Disposiciones.Add(nuevaDisposicion);
                    }
                }

                // Guardar las disposiciones en la base de datos
                _context.SaveChanges();

                // Enviar notificación al supervisor si es necesario
                _emailService.SendEmailToIncomingSupervisorAsync();

                return Ok(new { message = "Químicos guardados exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al guardar los químicos: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al guardar los químicos." });
            }
        }

        private string ValidarQuimico(Quimico quimico, ref bool isRejected)
        {
            string commentsText = "";

            if (quimico.Packaging != "OK")
            {
                isRejected = true;
                commentsText += "Empaque en mal estado.\n";
            }
            if (quimico.Fifo != "Sí")
            {
                isRejected = true;
                commentsText += "No se está cumpliendo FIFO.\n";
            }
            if (quimico.Mixed != "No")
            {
                isRejected = true;
                commentsText += "Químicos mezclados.\n";
            }
            if (quimico.QcSeal != "Sí")
            {
                isRejected = true;
                commentsText += "No cuenta con sello de calidad.\n";
            }
            if (quimico.Clean != "Limpio")
            {
                isRejected = true;
                commentsText += "Limpieza del químico en mal estado.\n";
            }

            if (quimico.Expiration < DateTime.Now)
            {
                isRejected = true;
                commentsText += "Químico caducado\n"; 
            }

            quimico.Result = isRejected ? "Rechazado" : "Aceptado";

            if (!isRejected && quimico.Expiration.HasValue)
            {
                int daysToExpire = (int)Math.Ceiling((quimico.Expiration.Value - DateTime.Now).TotalDays);
                if (daysToExpire <= 30)
                {
                    commentsText += "Químico próximo a vencer\n"; 
                    quimico.Result = "Próximo a vencer";
                }
            }

            return commentsText;
        }

    }
}
