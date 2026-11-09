using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_MonteroIsaac.Data;
using ProyectoFinal_MonteroIsaac.Models;

namespace ProyectoFinal_MonteroIsaac.Controllers
{
    // HU-A04: CRUD de carreras, solo rol Administrador
    [Authorize(Roles = "Administrador")]
    public class CarrerasController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CarrerasController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index() =>
            View(await _db.Carreras.Include(c => c.Cursos).OrderBy(c => c.Codigo).ToListAsync());

        public IActionResult Create() => View(new Carrera());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Carrera carrera)
        {
            carrera.Codigo = carrera.Codigo?.Trim().ToUpper();
            if (await _db.Carreras.AnyAsync(c => c.Codigo == carrera.Codigo))
                ModelState.AddModelError(nameof(carrera.Codigo), "El código de la carrera ya existe.");
            if (!ModelState.IsValid) return View(carrera);

            _db.Carreras.Add(carrera);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Carrera creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var carrera = await _db.Carreras.FindAsync(id);
            return carrera == null ? NotFound() : View(carrera);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Carrera carrera)
        {
            if (id != carrera.Id) return NotFound();
            carrera.Codigo = carrera.Codigo?.Trim().ToUpper();
            if (await _db.Carreras.AnyAsync(c => c.Codigo == carrera.Codigo && c.Id != id))
                ModelState.AddModelError(nameof(carrera.Codigo), "El código de la carrera ya existe.");
            if (!ModelState.IsValid) return View(carrera);

            _db.Update(carrera);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Carrera actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var carrera = await _db.Carreras.Include(c => c.Cursos).Include(c => c.Estudiantes)
                .FirstOrDefaultAsync(c => c.Id == id);
            return carrera == null ? NotFound() : View(carrera);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var carrera = await _db.Carreras.Include(c => c.Cursos).Include(c => c.Estudiantes)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (carrera == null) return NotFound();

            if (carrera.Cursos.Any() || carrera.Estudiantes.Any())
            {
                TempData["Error"] = "No se puede eliminar una carrera con cursos o estudiantes asociados. Puede inactivarla.";
                return RedirectToAction(nameof(Index));
            }
            _db.Carreras.Remove(carrera);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Carrera eliminada.";
            return RedirectToAction(nameof(Index));
        }
    }
}
