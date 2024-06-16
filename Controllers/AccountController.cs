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

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string employeeNumber)
        {
            var auditor = _context.Auditors.SingleOrDefault(a => a.EmployeeNumber == employeeNumber);
            if (auditor != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, auditor.Name),
                    new Claim("EmployeeNumber", auditor.EmployeeNumber),
                    new Claim(ClaimTypes.Role, auditor.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                HttpContext.Session.SetString("AuditorName", auditor.Name);

                if (auditor.Role == "IncomingSupervisor")
                {
                    return RedirectToAction("IndexIncoming", "Supervisor");
                }
                else if (auditor.Role == "StorageSupervisor")
                {
                    return RedirectToAction("IndexStorage", "Supervisor");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

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
