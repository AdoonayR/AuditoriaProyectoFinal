using Microsoft.AspNetCore.Mvc;
using AuditoriaQuimicos.Models;
using AuditoriaQuimicos.Data;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System;

namespace AuditoriaQuimicos.Controllers
{
    [Authorize] // Requiere que el usuario esté autorizado para acceder a este controlador
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Acción para mostrar la vista principal (Index)
        public IActionResult Index()
        {
            return View();
        }

        // Acción para guardar una lista de químicos
        [HttpPost]
        [Route("api/quimicos")]
        public IActionResult SaveQuimicos([FromBody] List<Quimico> quimicos)
        {
            // Obtener el nombre del auditor desde la sesión
            var auditor = HttpContext.Session.GetString("AuditorName");
            if (string.IsNullOrEmpty(auditor))
            {
                // Si el auditor no está autenticado, retornar una respuesta no autorizada
                return Unauthorized(new { message = "Auditor no autenticado" });
            }

            // Verificar si la lista de químicos es nula o está vacía
            if (quimicos == null || quimicos.Count == 0)
            {
                // Si la lista es nula o está vacía, retornar una respuesta de solicitud incorrecta
                return BadRequest(new { message = "Lista de químicos es nula o está vacía" });
            }

            // Iterar a través de cada químico en la lista
            foreach (var quimico in quimicos)
            {
                // Establecer la fecha actual como fecha de auditoría
                quimico.AuditDate = DateTime.Now;
                // Establecer el nombre del auditor
                quimico.Auditor = auditor;
                // Si los comentarios son nulos, inicializarlos como una cadena vacía
                quimico.Comments = quimico.Comments ?? "";

                // Lógica para determinar el resultado del químico
                if (quimico.Packaging != "OK" || quimico.Expiration < DateTime.Now || quimico.Fifo != "Sí" ||
                    quimico.Mixed != "No" || quimico.QcSeal != "Sí" || quimico.Clean != "Limpio")
                {
                    // Si alguna condición no se cumple, el químico es rechazado
                    quimico.Result = "Rechazado";
                }
                else if ((quimico.Expiration - DateTime.Now).Value.Days <= 30)
                {
                    // Si el químico está próximo a vencer, se marca como "Próximo a vencer"
                    quimico.Result = "Próximo a vencer";
                }
                else
                {
                    // Si todas las condiciones se cumplen, el químico es aceptado
                    quimico.Result = "Aceptado";
                }
            }

            // Agregar los químicos a la base de datos
            _context.Quimicos.AddRange(quimicos);
            // Guardar los cambios en la base de datos
            _context.SaveChanges();

            // Retornar una respuesta exitosa
            return Ok(new { message = "Químicos guardados exitosamente" });
        }
    }
}
