using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_MonteroIsaac.Data;
using ProyectoFinal_MonteroIsaac.Models;

namespace ProyectoFinal_MonteroIsaac.Controllers
{
    // HU-A07: CRUD de docentes con búsqueda y conteo de cursos
    [Authorize(Roles = "Administrador")]
    public class DocentesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public DocentesController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index(string q)
        {
            var query = _db.Docentes.Include(d => d.Cursos).AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(d => d.Nombre.Contains(q) || d.Especialidad.Contains(q));
            ViewData["q"] = q;
            return View(await query.OrderBy(d => d.Nombre).ToListAsync());
        }

        public IActionResult Create() => View(new Docente());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Docente docente)
        {
            if (!ModelState.IsValid) return View(docente);
            _db.Docentes.Add(docente);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Docente creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var d = await _db.Docentes.FindAsync(id);
            return d == null ? NotFound() : View(d);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Docente docente)
        {
            if (id != docente.Id) return NotFound();
            if (!ModelState.IsValid) return View(docente);
            _db.Update(docente);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Docente actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var d = await _db.Docentes.Include(x => x.Cursos).FirstOrDefaultAsync(x => x.Id == id);
            return d == null ? NotFound() : View(d);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var d = await _db.Docentes.Include(x => x.Cursos).FirstOrDefaultAsync(x => x.Id == id);
            if (d == null) return NotFound();
            if (d.Cursos.Any())
            {
                TempData["Error"] = "No se puede eliminar un docente con cursos asignados.";
                return RedirectToAction(nameof(Index));
            }
            _db.Docentes.Remove(d);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Docente eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
