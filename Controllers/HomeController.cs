using Microsoft.AspNetCore.Mvc;
using AuditoriaQuimicos.Models;
using AuditoriaQuimicos.Data;
using System.Collections.Generic;
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
            return View();
        }

        [HttpPost]
        [Route("api/quimicos")]
        public IActionResult SaveQuimicos([FromBody] List<Quimico> quimicos)
        {
            foreach (var quimico in quimicos)
            {
                quimico.Comments = quimico.Comments ?? ""; 
                quimico.Result = quimico.Result ?? ""; 
            }

            _context.Quimicos.AddRange(quimicos);
            _context.SaveChanges();

            return Ok(new { message = "Químicos guardados exitosamente" });
        }
    }
}
