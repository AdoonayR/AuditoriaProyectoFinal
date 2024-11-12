using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AuditoriaQuimicos.Data;
using AuditoriaQuimicos.Models;
using Microsoft.EntityFrameworkCore;

namespace AuditoriaQuimicos.Controllers
{
    public class DisposicionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DisposicionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Método para listar químicos según el estado
        public async Task<IActionResult> IndexDisposicion(EstadoDisposicion? estado)
        {
            var quimicosRechazados = _context.Disposiciones
                .Include(d => d.Quimico)
                .AsQueryable();

            if (estado.HasValue)
            {
                quimicosRechazados = quimicosRechazados.Where(d => d.Estado == estado.Value);
            }
            else
            {
                quimicosRechazados = quimicosRechazados.Where(d => d.Estado == EstadoDisposicion.Pendiente || d.Estado == EstadoDisposicion.EnRevision);
            }

            ViewBag.Titulo = estado == EstadoDisposicion.FueraDelAlmacen ? "Químicos Fuera del Almacén" : "Químicos Pendientes de Disposición";
            ViewBag.EsFueraDelAlmacen = estado == EstadoDisposicion.FueraDelAlmacen;
            var resultado = await quimicosRechazados.ToListAsync();

            return View("IndexDisposicion", resultado);
        }

        [HttpPost]
        public async Task<IActionResult> CompletarDisposicion(int id)
        {
            var disposicion = await _context.Disposiciones.FirstOrDefaultAsync(d => d.Id == id);
            if (disposicion == null) return NotFound();

            disposicion.Estado = EstadoDisposicion.FueraDelAlmacen;
            disposicion.FechaActualizacion = DateTime.Now;
            _context.Update(disposicion);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(IndexDisposicion), new { estado = EstadoDisposicion.FueraDelAlmacen });
        }

        [HttpPost]
        [Route("Disposicion/UpdateEstado")]
        public IActionResult UpdateEstado(int id, string estado)
        {
            var disposicion = _context.Disposiciones.FirstOrDefault(d => d.Id == id);
            if (disposicion == null) return NotFound();

            if (Enum.TryParse(estado, out EstadoDisposicion parsedEstado))
            {
                disposicion.Estado = parsedEstado;
                disposicion.FechaActualizacion = DateTime.Now;
                _context.SaveChanges();
            }
            else
            {
                return BadRequest(new { message = "Estado inválido" });
            }

            return Ok(new { message = "Estado actualizado correctamente" });
        }
        public async Task<IActionResult> IndexFueraDelAlmacen()
        {
            var quimicosFueraDelAlmacen = await _context.Disposiciones
                .Include(d => d.Quimico)
                .Where(d => d.Estado == EstadoDisposicion.FueraDelAlmacen)
                .ToListAsync();

            ViewBag.Titulo = "Químicos Fuera del Almacén";
            return View("IndexFueraDelAlmacen", quimicosFueraDelAlmacen);
        }

    }
}
