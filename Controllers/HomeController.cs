using Microsoft.AspNetCore.Mvc;
using AuditoriaQuimicos.Models;
using AuditoriaQuimicos.Data;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System;

namespace AuditoriaQuimicos.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
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
                quimico.AuditDate = DateTime.Now; // Establecer la fecha actual como fecha de auditoría
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
