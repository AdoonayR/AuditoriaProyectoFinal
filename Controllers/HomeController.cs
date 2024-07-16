using Microsoft.AspNetCore.Mvc;
using AuditoriaQuimicos.Models;
using AuditoriaQuimicos.Data;
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

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var quimicos = _context.Quimicos.ToList();
            var quimicosProximos = quimicos.Where(q => q.Expiration.HasValue && q.Expiration.Value.Month == DateTime.Now.AddMonths(-1).Month && q.Expiration.Value.Year == DateTime.Now.AddMonths(-1).Year).ToList();
            ViewData["QuimicosProximos"] = quimicosProximos;
            return View(quimicos);
        }

        public IActionResult Prioridades()
        {
            var quimicosProximos = _context.Quimicos
                .Where(q => q.Expiration.HasValue && q.Expiration.Value.Month == DateTime.Now.AddMonths(-1).Month && q.Expiration.Value.Year == DateTime.Now.AddMonths(-1).Year)
                .ToList();

            return View(quimicosProximos);
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

                    // Convertir la fecha de MM/DD/YYYY a un DateTime para guardar correctamente
                    if (DateTime.TryParseExact(quimico.ExpirationString, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime expirationDate))
                    {
                        quimico.Expiration = expirationDate;
                    }
                    else
                    {
                        _logger.LogWarning("Formato de fecha de caducidad inválido.");
                        return BadRequest(new { message = "Formato de fecha de caducidad inválido" });
                    }

                    bool isExpiringSoon = (quimico.Expiration.Value.Month == DateTime.Now.Month) && (quimico.Expiration.Value.Year == DateTime.Now.Year);
                    bool isExpired = quimico.Expiration < DateTime.Now;
                    bool hasIssues = quimico.Packaging != "OK" || quimico.Fifo != "Sí" || quimico.Mixed != "No" || quimico.QcSeal != "Sí" || quimico.Clean != "Limpio";

                    if (quimico.Packaging != "OK")
                    {
                        quimico.Comments += "\nEmpaque en mal estado.";
                    }
                    if (isExpired)
                    {
                        quimico.Comments += $"\nQuímico caducado hace: {Math.Floor((DateTime.Now - quimico.Expiration.Value).TotalDays)} días.";
                    }
                    if (quimico.Fifo != "Sí")
                    {
                        quimico.Comments += "\nNo se está cumpliendo FIFO.";
                    }
                    if (quimico.Mixed != "No")
                    {
                        quimico.Comments += "\nQuímicos mezclados.";
                    }
                    if (quimico.QcSeal != "Sí")
                    {
                        quimico.Comments += "\nNo cuenta con sello de calidad.";
                    }
                    if (quimico.Clean != "Limpio")
                    {
                        quimico.Comments += "\nLimpieza del químico en mal estado.";
                    }
                    if (isExpiringSoon && !isExpired)
                    {
                        quimico.Comments += $"\nQuímico próximo a vencer en {Math.Floor((quimico.Expiration.Value - DateTime.Now).TotalDays)} días.";
                    }

                    if (hasIssues)
                    {
                        quimico.Result = "Rechazado";
                    }
                    else if (isExpiringSoon)
                    {
                        quimico.Result = "Próximo a vencer";
                    }
                    else
                    {
                        quimico.Result = "Aceptado";
                    }
                }

                _logger.LogInformation("Agregando químicos al contexto.");
                _context.Quimicos.AddRange(quimicos);
                _logger.LogInformation("Guardando cambios en la base de datos.");
                var changes = _context.SaveChanges();
                _logger.LogInformation($"Se han guardado {changes} cambios en la base de datos.");

                if (changes > 0)
                {
                    return Ok(new { message = "Químicos guardados exitosamente" });
                }
                else
                {
                    _logger.LogError("No se pudieron guardar los cambios.");
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "No se pudieron guardar los cambios." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al guardar químicos: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al guardar los químicos." });
            }
        }
    }
}
