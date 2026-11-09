using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_MonteroIsaac.Data;
using ProyectoFinal_MonteroIsaac.Models;

namespace ProyectoFinal_MonteroIsaac.Controllers
{
    // HU-A03: gestión de estudiantes
    [Authorize(Roles = "Administrador")]
    public class EstudiantesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public EstudiantesController(ApplicationDbContext db) => _db = db;

        private async Task CargarCarreras(int? sel = null) =>
            ViewBag.Carreras = new SelectList(await _db.Carreras.OrderBy(c => c.Nombre).ToListAsync(), "Id", "Nombre", sel);

        public async Task<IActionResult> Index(string q)
        {
            var query = _db.Estudiantes.Include(e => e.Carrera).AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(e => e.Carne.Contains(q) || e.NombreCompleto.Contains(q) || e.Carrera.Nombre.Contains(q) || e.Carrera.Codigo.Contains(q));
            ViewData["q"] = q;
            return View(await query.OrderBy(e => e.NombreCompleto).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var e = await _db.Estudiantes.Include(x => x.Carrera)
                .Include(x => x.Matriculas).ThenInclude(m => m.Curso).FirstOrDefaultAsync(x => x.Id == id);
            return e == null ? NotFound() : View(e);
        }

        public async Task<IActionResult> Create()
        {
            await CargarCarreras();
            return View(new Estudiante());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Estudiante est)
        {
            ModelState.Remove(nameof(Estudiante.Carrera));
            ModelState.Remove(nameof(Estudiante.User));
            if (await _db.Estudiantes.AnyAsync(e => e.Carne == est.Carne))
                ModelState.AddModelError(nameof(est.Carne), "El carné ya existe.");
            if (!ModelState.IsValid) { await CargarCarreras(est.CarreraId); return View(est); }
            _db.Estudiantes.Add(est);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Estudiante creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var e = await _db.Estudiantes.FindAsync(id);
            if (e == null) return NotFound();
            await CargarCarreras(e.CarreraId);
            return View(e);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Estudiante est)
        {
            if (id != est.Id) return NotFound();
            ModelState.Remove(nameof(Estudiante.Carrera));
            ModelState.Remove(nameof(Estudiante.User));
            if (await _db.Estudiantes.AnyAsync(e => e.Carne == est.Carne && e.Id != id))
                ModelState.AddModelError(nameof(est.Carne), "El carné ya existe.");
            if (!ModelState.IsValid) { await CargarCarreras(est.CarreraId); return View(est); }
            _db.Update(est);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Estudiante actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var e = await _db.Estudiantes.Include(x => x.Carrera).Include(x => x.Matriculas).FirstOrDefaultAsync(x => x.Id == id);
            return e == null ? NotFound() : View(e);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var e = await _db.Estudiantes.Include(x => x.Matriculas).FirstOrDefaultAsync(x => x.Id == id);
            if (e == null) return NotFound();
            if (e.Matriculas.Any())
            {
                // Con matrícula activa no se elimina: solo se inactiva
                e.Activo = false;
                await _db.SaveChangesAsync();
                TempData["Ok"] = "El estudiante tiene matrícula activa: no se eliminó, se marcó como Inactivo.";
                return RedirectToAction(nameof(Index));
            }
            _db.Estudiantes.Remove(e);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Estudiante eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
