using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_MonteroIsaac.Data;
using ProyectoFinal_MonteroIsaac.Models;

namespace ProyectoFinal_MonteroIsaac.Controllers
{
    // HU-A08: consulta y exportación de matrículas
    [Authorize(Roles = "Administrador")]
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _db;
        public MatriculasController(ApplicationDbContext db) => _db = db;

        private IQueryable<Matricula> Filtrar(int? periodoId, int? carreraId, int? cursoId)
        {
            var q = _db.Matriculas.Include(m => m.Estudiante).Include(m => m.Curso).ThenInclude(c => c.Carrera).AsQueryable();
            if (periodoId != null) q = q.Where(m => m.PeriodoId == periodoId);
            if (carreraId != null) q = q.Where(m => m.Curso.CarreraId == carreraId);
            if (cursoId != null) q = q.Where(m => m.CursoId == cursoId);
            return q.OrderBy(m => m.Estudiante.Carne).ThenBy(m => m.Curso.Codigo);
        }

        public async Task<IActionResult> Index(int? periodoId, int? carreraId, int? cursoId)
        {
            ViewBag.Periodos = new SelectList(await _db.Periodos.OrderByDescending(p => p.FechaApertura).ToListAsync(), "Id", "Nombre", periodoId);
            ViewBag.Carreras = new SelectList(await _db.Carreras.OrderBy(c => c.Codigo).ToListAsync(), "Id", "Codigo", carreraId);
            ViewBag.Cursos = new SelectList(await _db.Cursos.OrderBy(c => c.Codigo).ToListAsync(), "Id", "Codigo", cursoId);
            ViewBag.PeriodoId = periodoId; ViewBag.CarreraId = carreraId; ViewBag.CursoId = cursoId;
            return View(await Filtrar(periodoId, carreraId, cursoId).ToListAsync());
        }

        // La exportación respeta los filtros aplicados
        public async Task<IActionResult> Exportar(int? periodoId, int? carreraId, int? cursoId)
        {
            var lista = await Filtrar(periodoId, carreraId, cursoId).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Carné,Estudiante,Carrera,Curso,Créditos,Fecha");
            foreach (var m in lista)
                sb.AppendLine($"{m.Estudiante.Carne},\"{m.Estudiante.NombreCompleto}\",{m.Curso.Carrera.Codigo},{m.Curso.Codigo},{m.Curso.Creditos},{m.Fecha:dd/MM/yyyy}");
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", "matriculas.csv");
        }
    }
}
