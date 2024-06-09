using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using AuditoriaQuimicos.Data;

namespace AuditoriaQuimicos.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string employeeNumber)
        {
            // Validar el número de empleado
            var auditor = _context.Auditors.SingleOrDefault(a => a.EmployeeNumber == employeeNumber);
            if (auditor != null)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, auditor.Name),
            new Claim("EmployeeNumber", auditor.EmployeeNumber)
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                HttpContext.Session.SetString("AuditorName", auditor.Name);

                return RedirectToAction("Index", "Home");
            }

            // Si el número de empleado es incorrecto, mostrar un mensaje de error
            ModelState.AddModelError(string.Empty, "Número de empleado incorrecto");
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("AuditorName");
            return RedirectToAction("Login", "Account");
        }
    }
}
