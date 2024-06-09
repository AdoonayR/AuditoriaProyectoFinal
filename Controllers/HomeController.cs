using Microsoft.AspNetCore.Mvc;
using AuditoriaQuimicos.Models;
using AuditoriaQuimicos.Data;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace AuditoriaQuimicos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Verificar si el auditor está autenticado
            var auditorName = HttpContext.Session.GetString("AuditorName");
            if (string.IsNullOrEmpty(auditorName))
            {
                // Si no está autenticado, redirigir al login
                return RedirectToAction("Login", "Account");
            }

            // Si está autenticado, renderizar la vista
            return View();
        }

        [HttpGet]
        [Route("api/getAuditorName")]
        public IActionResult GetAuditorName()
        {
            var auditorName = HttpContext.Session.GetString("AuditorName");
            return Ok(new { auditorName });
        }

        [HttpPost]
        [Route("api/quimicos")]
        public IActionResult SaveQuimicos([FromBody] List<Quimico> quimicos)
        {
            var auditor = HttpContext.Session.GetString("AuditorName");
            if (string.IsNullOrEmpty(auditor))
            {
                return Unauthorized(new { message = "Auditor no autenticado" });
            }

            foreach (var quimico in quimicos)
            {
                quimico.Auditor = auditor;
                quimico.Comments = quimico.Comments ?? "";
                quimico.Result = quimico.Result ?? "";
            }

            _context.Quimicos.AddRange(quimicos);
            _context.SaveChanges();

            return Ok(new { message = "Químicos guardados exitosamente" });
        }
    }
}
