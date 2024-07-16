using Microsoft.AspNetCore.Mvc;
using AuditoriaQuimicos.Models;
using AuditoriaQuimicos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace AuditoriaQuimicos.Controllers
{
    [Authorize] // Requiere que el usuario esté autenticado para acceder a los métodos del controlador
    public class PrioridadesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrioridadesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet] // Maneja solicitudes GET
        public async Task<IActionResult> IndexPrioridades()
        {
            var currentDate = DateTime.Now; // Obtiene la fecha y hora actual
            var lastMonth = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-1); // Obtiene el primer día del mes anterior
            var startDate = lastMonth; // Establece la fecha de inicio como el primer día del mes anterior
            var endDate = lastMonth.AddMonths(1).AddDays(-1); // Establece la fecha de fin como el último día del mes anterior

            // Obtiene los químicos cuya fecha de expiración esté entre el primer y el último día del mes anterior
            var quimicos = await _context.Quimicos
                .Where(q => q.Expiration != null && q.Expiration >= startDate && q.Expiration <= endDate)
                .ToListAsync();

            return View(quimicos); // Retorna la vista con la lista de químicos
        }

        [HttpGet] // Maneja solicitudes GET
        public async Task<IActionResult> GetPrioridades()
        {
            var currentDate = DateTime.Now; // Obtiene la fecha y hora actual
            var lastMonth = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-1); // Obtiene el primer día del mes anterior
            var startDate = lastMonth; // Establece la fecha de inicio como el primer día del mes anterior
            var endDate = lastMonth.AddMonths(1).AddDays(-1); // Establece la fecha de fin como el último día del mes anterior

            // Obtiene los químicos cuya fecha de expiración esté entre el primer y el último día del mes anterior
            var quimicos = await _context.Quimicos
                .Where(q => q.Expiration != null && q.Expiration >= startDate && q.Expiration <= endDate)
                .ToListAsync();

            return Json(quimicos); // Retorna la lista de químicos en formato JSON
        }
    }
}
