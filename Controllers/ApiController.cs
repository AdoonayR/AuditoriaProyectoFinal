using Microsoft.AspNetCore.Mvc;
using AuditoriaQuimicos.Data;
using AuditoriaQuimicos.Models;
using System;

namespace AuditoriaQuimicos.Controllers
{
    [ApiController] // Indica que este controlador responde a solicitudes de API
    [Route("api/[controller]")] // Define la ruta base para las solicitudes de este controlador
    public class ApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Acción para obtener el nombre del auditor desde la sesión
        [HttpGet("getAuditorName")]
        public IActionResult GetAuditorName()
        {
            // Obtener el nombre del auditor desde la sesión
            var auditorName = HttpContext.Session.GetString("AuditorName");

            // Verificar si el nombre del auditor está vacío o es nulo
            if (string.IsNullOrEmpty(auditorName))
            {
                // Si está vacío o es nulo, retornar una respuesta no autorizada
                return Unauthorized(new { message = "Auditor no autenticado" });
            }

            // Si se encuentra el nombre del auditor, retornar una respuesta OK con el nombre
            return Ok(new { auditorName });
        }
    }
}
