using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_MonteroIsaac.Data;
using ProyectoFinal_MonteroIsaac.Models;

namespace ProyectoFinal_MonteroIsaac.Controllers
{
    // HU-D01: consulta (solo lectura) de cursos asignados y estudiantes matriculados
    [Authorize(Roles = "Docente")]
    public class DocenteController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;

        public DocenteController(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _um = um;
        }

        public async Task<IActionResult> Index(int? cursoId)
        {
            var email = (await _um.GetUserAsync(User))?.Email;
            var docente = await _db.Docentes.FirstOrDefaultAsync(d => d.Correo == email);
            ViewBag.Docente = docente;
            if (docente == null) return View(new List<Curso>());

            var cursos = await _db.Cursos.Where(c => c.DocenteId == docente.Id).OrderBy(c => c.Codigo).ToListAsync();
            var seleccionado = cursos.FirstOrDefault(c => c.Id == cursoId) ?? cursos.FirstOrDefault();
            ViewBag.Seleccionado = seleccionado;
            if (seleccionado != null)
            {
                var periodo = await _db.Periodos.OrderByDescending(p => p.FechaApertura).FirstOrDefaultAsync();
                var pid = periodo?.Id ?? 0;
                ViewBag.Estudiantes = await _db.Matriculas.Include(m => m.Estudiante)
                    .Where(m => m.CursoId == seleccionado.Id && m.PeriodoId == pid)
                    .OrderBy(m => m.Estudiante.Carne).Select(m => m.Estudiante).ToListAsync();
            }
            return View(cursos);
        }
    }
}
