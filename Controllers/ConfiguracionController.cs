using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_MonteroIsaac.Data;
using ProyectoFinal_MonteroIsaac.Models;
using ProyectoFinal_MonteroIsaac.ViewModels;

namespace ProyectoFinal_MonteroIsaac.Controllers
{
    // HU-A09 (periodo de matrícula) y HU-A10 (usuarios y roles)
    [Authorize(Roles = "Administrador")]
    public class ConfiguracionController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;

        public ConfiguracionController(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _um = um;
        }

        private Task<Periodo> PeriodoVigente() => _db.Periodos.OrderByDescending(p => p.FechaApertura).FirstOrDefaultAsync();

        public async Task<IActionResult> Index()
        {
            var vm = new ConfiguracionViewModel { Periodo = await PeriodoVigente() };
            foreach (var u in await _um.Users.OrderBy(u => u.Email).ToListAsync())
            {
                var roles = await _um.GetRolesAsync(u);
                vm.Usuarios.Add(new UsuarioRolViewModel { Id = u.Id, Email = u.Email, Rol = roles.FirstOrDefault() ?? "Estudiante", Activo = u.Activo });
            }
            return View(vm);
        }

        // Abrir o cerrar el periodo; se registran las fechas
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePeriodo()
        {
            var p = await PeriodoVigente();
            if (p == null) return NotFound();
            p.Abierto = !p.Abierto;
            if (p.Abierto) p.FechaApertura = DateTime.Now; else p.FechaCierre = DateTime.Now;
            await _db.SaveChangesAsync();
            TempData["Ok"] = p.Abierto ? "Periodo de matrícula abierto." : "Periodo de matrícula cerrado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarPeriodo(int id, string nombre, DateTime fechaApertura, DateTime fechaCierre, int limiteCreditos)
        {
            var p = await _db.Periodos.FindAsync(id);
            if (p == null) return NotFound();
            if (limiteCreditos < 1 || limiteCreditos > 30 || string.IsNullOrWhiteSpace(nombre) || fechaCierre < fechaApertura)
            {
                TempData["Error"] = "Datos inválidos: el límite debe estar entre 1 y 30 y la fecha de cierre no puede ser anterior a la apertura.";
                return RedirectToAction(nameof(Index));
            }
            p.Nombre = nombre.Trim();
            p.FechaApertura = fechaApertura;
            p.FechaCierre = fechaCierre;
            p.LimiteCreditos = limiteCreditos;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Periodo actualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarRol(string id, string rol)
        {
            if (!SeedData.Roles.Contains(rol)) return BadRequest();
            var u = await _um.FindByIdAsync(id);
            if (u == null) return NotFound();
            var actuales = await _um.GetRolesAsync(u);

            if (actuales.Contains("Administrador") && rol != "Administrador")
            {
                var admins = await _um.GetUsersInRoleAsync("Administrador");
                if (admins.Count <= 1)
                {
                    TempData["Error"] = "No se puede quitar el rol al último administrador del sistema.";
                    return RedirectToAction(nameof(Index));
                }
            }
            // Cada usuario tiene un único rol activo
            await _um.RemoveFromRolesAsync(u, actuales);
            await _um.AddToRoleAsync(u, rol);
            TempData["Ok"] = $"Rol de {u.Email} cambiado a {rol}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUsuario(string id)
        {
            var u = await _um.FindByIdAsync(id);
            if (u == null) return NotFound();
            if (u.Id == _um.GetUserId(User))
            {
                TempData["Error"] = "No puede desactivar su propia cuenta.";
                return RedirectToAction(nameof(Index));
            }
            u.Activo = !u.Activo;
            await _um.UpdateAsync(u);
            if (!u.Activo) await _um.UpdateSecurityStampAsync(u);
            TempData["Ok"] = u.Activo ? "Usuario activado." : "Usuario desactivado: ya no puede iniciar sesión.";
            return RedirectToAction(nameof(Index));
        }
    }
}
