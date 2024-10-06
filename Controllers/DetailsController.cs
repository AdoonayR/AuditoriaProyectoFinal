using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using AuditoriaQuimicos.Data;
using AuditoriaQuimicos.Models;
using System.Globalization;

namespace AuditoriaQuimicos.Controllers
{
    public class DetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DetailsController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Acción para mostrar los detalles de los químicos para una fecha específica
        public IActionResult Details(string date)
        {
            if (string.IsNullOrEmpty(date))
            {
                return BadRequest("Fecha no proporcionada.");
            }

            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime auditDate))
            {
                return BadRequest("Fecha inválida.");
            }

            var quimicos = _context.Quimicos
                .Where(q => q.AuditDate.HasValue && q.AuditDate.Value.Date == auditDate)
                .ToList();

            if (!quimicos.Any())
            {
                return NotFound("No se encontraron químicos para la fecha proporcionada.");
            }

            // Indicar explícitamente la ruta de la vista
            return View("~/Views/Supervisor/Details.cshtml", quimicos);
        }
    }

}
