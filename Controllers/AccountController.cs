//Usuarios de prueba
//| Rol           | Correo                | Contraseña  |
//| Administrador | admin@fidelitas.ac.cr | Admin123    |
//| Docente       | solano@correo.edu     | Docente1    |
//| Estudiante    | ana.mora@correo.edu   | Estudiante1 |

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_MonteroIsaac.Data;
using ProyectoFinal_MonteroIsaac.Models;
using ProyectoFinal_MonteroIsaac.ViewModels;

namespace ProyectoFinal_MonteroIsaac.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        private async Task CargarCarreras(int? seleccionada = null)
        {
            // HU-E03: solo carreras activas, con duración y cantidad de cursos
            var carreras = await _db.Carreras.Where(c => c.Activa)
                .OrderBy(c => c.Nombre)
                .Select(c => new
                {
                    c.Id,
                    Texto = c.Nombre + " (" + c.DuracionAnios + " años, " + c.Cursos.Count + " cursos)"
                }).ToListAsync();
            ViewBag.Carreras = new SelectList(carreras, "Id", "Texto", seleccionada);
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            await CargarCarreras();
            return View(new RegisterViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var carrera = await _db.Carreras.FirstOrDefaultAsync(c => c.Id == model.CarreraId && c.Activa);
                if (carrera == null)
                    ModelState.AddModelError(nameof(model.CarreraId), "La carrera seleccionada no está disponible.");
            }

            if (!ModelState.IsValid)
            {
                await CargarCarreras(model.CarreraId);
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                NombreCompleto = model.NombreCompleto
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                {
                    var msg = e.Code switch
                    {
                        "DuplicateUserName" or "DuplicateEmail" => "El correo ya está registrado.",
                        "PasswordTooShort" => "La contraseña debe tener al menos 8 caracteres.",
                        "PasswordRequiresUpper" => "La contraseña debe incluir una mayúscula.",
                        "PasswordRequiresDigit" => "La contraseña debe incluir un número.",
                        _ => e.Description
                    };
                    ModelState.AddModelError(string.Empty, msg);
                }
                await CargarCarreras(model.CarreraId);
                return View(model);
            }

            // Rol Estudiante asignado automáticamente
            await _userManager.AddToRoleAsync(user, "Estudiante");

            // Si el administrador ya había creado el estudiante con ese correo, se vincula a la cuenta
            var estudiante = await _db.Estudiantes.FirstOrDefaultAsync(e => e.Correo == model.Email && e.UserId == null);
            if (estudiante != null)
            {
                estudiante.UserId = user.Id;
                estudiante.CarreraId = model.CarreraId.Value;
                await _db.SaveChangesAsync();
            }
            else
            {
                estudiante = new Estudiante
                {
                    UserId = user.Id,
                    NombreCompleto = model.NombreCompleto,
                    Correo = model.Email,
                    CarreraId = model.CarreraId.Value,
                    Carne = Guid.NewGuid().ToString("N").Substring(0, 12),
                    Activo = true
                };
                _db.Estudiantes.Add(estudiante);
                await _db.SaveChangesAsync();
                estudiante.Carne = $"{DateTime.Now.Year}-{estudiante.Id:D4}";
                await _db.SaveChangesAsync();
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            TempData["Ok"] = "Cuenta creada correctamente. ¡Bienvenido!";
            return RedirectToAction("Index", "Estudiante");
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            const string mensajeGenerico = "Credenciales inválidas.";
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.Activo)
            {
                ModelState.AddModelError(string.Empty, mensajeGenerico);
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Cuenta bloqueada temporalmente por intentos fallidos. Intente más tarde.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, mensajeGenerico);
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}
