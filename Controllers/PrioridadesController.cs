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
    [Authorize]
    public class PrioridadesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrioridadesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> IndexPrioridades()
        {
            var currentDate = DateTime.Now;
            var lastMonth = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-1);
            var startDate = lastMonth;
            var endDate = lastMonth.AddMonths(1).AddDays(-1);

            var quimicos = await _context.Quimicos
                .Where(q => q.Expiration != null && q.Expiration >= startDate && q.Expiration <= endDate)
                .ToListAsync();

            return View(quimicos);
        }

        [HttpGet]
        public async Task<IActionResult> GetPrioridades()
        {
            var currentDate = DateTime.Now;
            var lastMonth = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-1);
            var startDate = lastMonth;
            var endDate = lastMonth.AddMonths(1).AddDays(-1);

            var quimicos = await _context.Quimicos
                .Where(q => q.Expiration != null && q.Expiration >= startDate && q.Expiration <= endDate)
                .ToListAsync();

            return Json(quimicos);
        }
    }
}
