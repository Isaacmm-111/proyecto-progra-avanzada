using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_MonteroIsaac.Data;
using ProyectoFinal_MonteroIsaac.ViewModels;

namespace ProyectoFinal_MonteroIsaac.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AdminController(ApplicationDbContext db) => _db = db;

        // HU-A02: dashboard con datos actuales de la base de datos
        public async Task<IActionResult> Index()
        {
            var periodo = await _db.Periodos.OrderByDescending(p => p.FechaApertura).FirstOrDefaultAsync();
            var pid = periodo?.Id ?? 0;
            var vm = new DashboardViewModel
            {
                TotalEstudiantes = await _db.Estudiantes.CountAsync(),
                TotalCarreras = await _db.Carreras.CountAsync(),
                TotalCursos = await _db.Cursos.CountAsync(),
                TotalDocentes = await _db.Docentes.CountAsync(),
                PeriodoVigente = periodo?.Nombre
            };
            var datos = await _db.Carreras
                .Select(c => new { c.Nombre, Total = c.Cursos.SelectMany(x => x.Matriculas).Count(m => m.PeriodoId == pid) })
                .OrderByDescending(x => x.Total).ToListAsync();
            vm.MatriculasPorCarrera = datos.Select(x => new BarraItem { Etiqueta = x.Nombre, Valor = x.Total }).ToList();
            return View(vm);
        }
    }
}
