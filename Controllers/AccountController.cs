using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using AuditoriaQuimicos.Data;
using System.Linq;
using System.Collections.Generic;

namespace AuditoriaQuimicos.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Acción para mostrar la página de inicio de sesión
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Acción para procesar la solicitud de inicio de sesión
        [HttpPost]
        public async Task<IActionResult> Login(string employeeNumber)
        {
            if (string.IsNullOrEmpty(employeeNumber))
            {
                ModelState.AddModelError("", "El número de empleado es obligatorio.");
                return View();
            }

            // Buscar el auditor en la base de datos usando el número de empleado
            var auditor = _context.Auditors.SingleOrDefault(a => a.EmployeeNumber == employeeNumber);
            if (auditor != null)
            {
                // Crear una lista de claims (información sobre el usuario)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, auditor.Name),
                    new Claim("EmployeeNumber", auditor.EmployeeNumber),
                    new Claim(ClaimTypes.Role, auditor.Role)
                };

                // Crear una identidad de claims usando la lista de claims
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true // Establecer la autenticación como persistente
                };

                // Iniciar sesión del usuario en el esquema de autenticación de cookies
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                // Guardar el nombre del auditor en la sesión
                HttpContext.Session.SetString("AuditorName", auditor.Name);

                // Redirigir al usuario según su rol
                if (auditor.Role == "IncomingSupervisor")
                {
                    return RedirectToAction("IndexIncoming", "Supervisor");
                }
                else if (auditor.Role == "StorageSupervisor")
                {
                    return RedirectToAction("IndexStorage", "Supervisor");
                }
                else if (auditor.Role == "Auditor")
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }

            // Si el número de empleado es incorrecto, agregar un mensaje de error al modelo
            ModelState.AddModelError("", "Número de empleado no encontrado.");
            return View();
        }

        // Acción para procesar la solicitud de cierre de sesión
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Cerrar sesión del usuario
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Eliminar el nombre del auditor de la sesión
            HttpContext.Session.Remove("AuditorName");

            // Redirigir a la página de inicio de sesión
            return RedirectToAction("Login", "Account");
        }
    }
}
