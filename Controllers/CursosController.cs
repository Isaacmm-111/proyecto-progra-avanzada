using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_MonteroIsaac.Data;
using ProyectoFinal_MonteroIsaac.Models;

namespace ProyectoFinal_MonteroIsaac.Controllers
{
    // HU-A05 y HU-A06: CRUD de cursos con asignación de docente, horario y cupo
    [Authorize(Roles = "Administrador")]
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CursosController(ApplicationDbContext db) => _db = db;

        private async Task CargarListas(Curso c = null)
        {
            ViewBag.Carreras = new SelectList(await _db.Carreras.OrderBy(x => x.Nombre).ToListAsync(), "Id", "Nombre", c?.CarreraId);
            ViewBag.Docentes = new SelectList(await _db.Docentes.OrderBy(x => x.Nombre).ToListAsync(), "Id", "Nombre", c?.DocenteId);
        }

        private async Task ValidarNegocio(Curso curso)
        {
            curso.Codigo = curso.Codigo?.Trim().ToUpper();
            if (await _db.Cursos.AnyAsync(x => x.Codigo == curso.Codigo && x.Id != curso.Id))
                ModelState.AddModelError(nameof(curso.Codigo), "El código del curso ya existe.");

            // Un docente no puede tener dos cursos en el mismo horario
            if (curso.DocenteId != null && !string.IsNullOrWhiteSpace(curso.Horario))
            {
                var otros = await _db.Cursos.Where(x => x.DocenteId == curso.DocenteId && x.Id != curso.Id)
                    .Select(x => x.Horario).ToListAsync();
                if (otros.Any(h => Services.HorarioHelper.Choca(h, curso.Horario)))
                    ModelState.AddModelError(nameof(curso.Horario), "El docente ya tiene otro curso en ese horario.");
            }
        }

        public async Task<IActionResult> Index(string q, int? carreraId)
        {
            var query = _db.Cursos.Include(c => c.Carrera).Include(c => c.Docente).Include(c => c.Matriculas).AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(c => c.Nombre.Contains(q) || c.Codigo.Contains(q));
            if (carreraId != null)
                query = query.Where(c => c.CarreraId == carreraId);

            ViewBag.Carreras = new SelectList(await _db.Carreras.OrderBy(x => x.Nombre).ToListAsync(), "Id", "Nombre", carreraId);
            ViewData["q"] = q;
            return View(await query.OrderBy(c => c.Codigo).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            await CargarListas();
            return View(new Curso());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Curso curso)
        {
            ModelState.Remove(nameof(Curso.Carrera));
            ModelState.Remove(nameof(Curso.Docente));
            await ValidarNegocio(curso);
            if (!ModelState.IsValid)
            {
                await CargarListas(curso);
                return View(curso);
            }
            _db.Cursos.Add(curso);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Curso creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var curso = await _db.Cursos.FindAsync(id);
            if (curso == null) return NotFound();
            await CargarListas(curso);
            return View(curso);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Curso curso)
        {
            if (id != curso.Id) return NotFound();
            ModelState.Remove(nameof(Curso.Carrera));
            ModelState.Remove(nameof(Curso.Docente));
            await ValidarNegocio(curso);
            if (!ModelState.IsValid)
            {
                await CargarListas(curso);
                return View(curso);
            }
            _db.Update(curso);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Curso actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var curso = await _db.Cursos.Include(c => c.Carrera).Include(c => c.Docente).Include(c => c.Matriculas)
                .FirstOrDefaultAsync(c => c.Id == id);
            return curso == null ? NotFound() : View(curso);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var curso = await _db.Cursos.Include(c => c.Matriculas).FirstOrDefaultAsync(c => c.Id == id);
            if (curso == null) return NotFound();
            if (curso.Matriculas.Any())
            {
                TempData["Error"] = "No se puede eliminar un curso con estudiantes matriculados.";
                return RedirectToAction(nameof(Index));
            }
            _db.Cursos.Remove(curso);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Curso eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
