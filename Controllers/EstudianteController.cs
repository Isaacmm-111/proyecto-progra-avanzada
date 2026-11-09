using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_MonteroIsaac.Data;
using ProyectoFinal_MonteroIsaac.Models;
using ProyectoFinal_MonteroIsaac.Services;
using ProyectoFinal_MonteroIsaac.ViewModels;

namespace ProyectoFinal_MonteroIsaac.Controllers
{
    [Authorize(Roles = "Estudiante")]
    public class EstudianteController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public EstudianteController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private async Task<Estudiante> ObtenerEstudiante()
        {
            var userId = _userManager.GetUserId(User);
            return await _db.Estudiantes.Include(e => e.Carrera).FirstOrDefaultAsync(e => e.UserId == userId);
        }

        private Task<Periodo> PeriodoVigente() =>
            _db.Periodos.OrderByDescending(p => p.FechaApertura).FirstOrDefaultAsync();

        private Task<List<Matricula>> MatriculasActuales(int estudianteId, int periodoId) =>
            _db.Matriculas.Include(m => m.Curso).ThenInclude(c => c.Docente)
                .Where(m => m.EstudianteId == estudianteId && m.PeriodoId == periodoId)
                .OrderBy(m => m.Curso.Codigo).ToListAsync();

        // HU-E04: pantalla de inicio
        public async Task<IActionResult> Index()
        {
            var est = await ObtenerEstudiante();
            if (est == null) return NotFound();
            var periodo = await PeriodoVigente();
            var mats = periodo == null ? new List<Matricula>() : await MatriculasActuales(est.Id, periodo.Id);

            return View(new EstudianteHomeViewModel
            {
                NombreCompleto = est.NombreCompleto,
                Carne = est.Carne,
                Carrera = est.Carrera.Nombre,
                PeriodoVigente = periodo?.Nombre ?? "Sin periodo",
                PeriodoAbierto = periodo?.Abierto ?? false,
                CreditosMatriculados = mats.Sum(m => m.Curso.Creditos),
                LimiteCreditos = periodo?.LimiteCreditos ?? 18,
                Matriculas = mats
            });
        }

        // HU-E03: selección de carrera (solo carreras activas)
        public async Task<IActionResult> MiCarrera()
        {
            var est = await ObtenerEstudiante();
            if (est == null) return NotFound();
            ViewBag.CarreraActual = est.CarreraId;
            return View(await _db.Carreras.Include(c => c.Cursos).Where(c => c.Activa).OrderBy(c => c.Nombre).ToListAsync());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MiCarrera(int? carreraId)
        {
            if (carreraId == null)
            {
                TempData["Error"] = "Debe elegir una carrera para continuar.";
                return RedirectToAction(nameof(MiCarrera));
            }
            var est = await ObtenerEstudiante();
            var carrera = await _db.Carreras.FirstOrDefaultAsync(c => c.Id == carreraId && c.Activa);
            if (est == null || carrera == null)
            {
                TempData["Error"] = "La carrera seleccionada no está disponible.";
                return RedirectToAction(nameof(MiCarrera));
            }
            if (carrera.Id != est.CarreraId)
            {
                var tieneMatricula = await _db.Matriculas.AnyAsync(m => m.EstudianteId == est.Id);
                if (tieneMatricula)
                {
                    TempData["Error"] = "No puede cambiar de carrera mientras tenga cursos matriculados.";
                    return RedirectToAction(nameof(MiCarrera));
                }
                est.CarreraId = carrera.Id;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Matricular));
        }

        // HU-E05: catálogo de cursos de mi carrera
        public async Task<IActionResult> Matricular(string q, int? cuatrimestre)
        {
            var est = await ObtenerEstudiante();
            if (est == null) return NotFound();
            var periodo = await PeriodoVigente();
            var pid = periodo?.Id ?? 0;

            var baseQuery = _db.Cursos.Where(c => c.CarreraId == est.CarreraId);
            var query = baseQuery.Include(c => c.Docente).Include(c => c.Matriculas.Where(m => m.PeriodoId == pid)).AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(c => c.Nombre.Contains(q) || c.Codigo.Contains(q));
            if (cuatrimestre != null)
                query = query.Where(c => c.Cuatrimestre == cuatrimestre);

            var vm = new CatalogoViewModel
            {
                Carrera = est.Carrera.Nombre,
                Periodo = periodo?.Nombre,
                PeriodoAbierto = periodo?.Abierto ?? false,
                Busqueda = q,
                Cuatrimestre = cuatrimestre,
                Cuatrimestres = await baseQuery.Select(c => c.Cuatrimestre).Distinct().OrderBy(x => x).ToListAsync(),
                Cursos = await query.OrderBy(c => c.Cuatrimestre).ThenBy(c => c.Codigo).ToListAsync(),
                Matriculados = periodo == null ? new HashSet<int>() :
                    (await _db.Matriculas.Where(m => m.EstudianteId == est.Id && m.PeriodoId == periodo.Id)
                        .Select(m => m.CursoId).ToListAsync()).ToHashSet()
            };
            return View(vm);
        }

        // HU-E07: validaciones (cupo, choque de horario, límite de créditos, duplicados)
        private async Task<ResumenMatriculaViewModel> Validar(Estudiante est, Periodo periodo, int[] ids)
        {
            ids ??= Array.Empty<int>();
            var seleccion = await _db.Cursos.Include(c => c.Docente)
                .Include(c => c.Matriculas.Where(m => m.PeriodoId == periodo.Id))
                .Where(c => ids.Contains(c.Id) && c.CarreraId == est.CarreraId)
                .OrderBy(c => c.Codigo).ToListAsync();
            var actuales = await MatriculasActuales(est.Id, periodo.Id);

            var vm = new ResumenMatriculaViewModel
            {
                Periodo = periodo.Nombre,
                Cursos = seleccion,
                LimiteCreditos = periodo.LimiteCreditos,
                CreditosSeleccionados = seleccion.Sum(c => c.Creditos),
                CreditosAcumulados = actuales.Sum(m => m.Curso.Creditos) + seleccion.Sum(c => c.Creditos)
            };

            // Cupo
            var sinCupo = seleccion.Where(c => c.CuposDisponibles <= 0).Select(c => c.Codigo).ToList();
            vm.Validaciones.Add(sinCupo.Count == 0
                ? new ValidacionItem { Ok = true, Mensaje = "Hay cupo en todos los cursos" }
                : new ValidacionItem { Ok = false, Mensaje = "Sin cupo: " + string.Join(", ", sinCupo) });

            // Choques de horario (entre los seleccionados y con los ya matriculados)
            var choques = new List<string>();
            for (int i = 0; i < seleccion.Count; i++)
            {
                for (int j = i + 1; j < seleccion.Count; j++)
                    if (HorarioHelper.Choca(seleccion[i].Horario, seleccion[j].Horario))
                        choques.Add($"{seleccion[i].Codigo} y {seleccion[j].Codigo} ({seleccion[i].Horario})");
                foreach (var m in actuales.Where(m => m.CursoId != seleccion[i].Id))
                    if (HorarioHelper.Choca(seleccion[i].Horario, m.Curso.Horario))
                        choques.Add($"{seleccion[i].Codigo} y {m.Curso.Codigo} ({seleccion[i].Horario})");
            }
            if (choques.Count == 0) vm.Validaciones.Add(new ValidacionItem { Ok = true, Mensaje = "No hay choques de horario" });
            else foreach (var c in choques) vm.Validaciones.Add(new ValidacionItem { Ok = false, Mensaje = "Choque de horario: " + c });

            // Límite de créditos
            vm.Validaciones.Add(vm.CreditosAcumulados <= periodo.LimiteCreditos
                ? new ValidacionItem { Ok = true, Mensaje = $"Dentro del límite de {periodo.LimiteCreditos} créditos por periodo" }
                : new ValidacionItem { Ok = false, Mensaje = $"Excede el límite de {periodo.LimiteCreditos} créditos por periodo" });

            // Duplicados
            var yaMat = actuales.Select(m => m.CursoId).ToHashSet();
            var dup = seleccion.Where(c => yaMat.Contains(c.Id)).Select(c => c.Codigo).ToList();
            vm.Validaciones.Add(dup.Count == 0
                ? new ValidacionItem { Ok = true, Mensaje = "Ningún curso está duplicado" }
                : new ValidacionItem { Ok = false, Mensaje = "Curso ya matriculado: " + string.Join(", ", dup) });

            return vm;
        }

        // HU-E06: resumen antes de confirmar
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Resumen(int[] cursoIds)
        {
            var est = await ObtenerEstudiante();
            var periodo = await PeriodoVigente();
            if (est == null || periodo == null) return NotFound();
            if (!periodo.Abierto)
            {
                TempData["Error"] = "El periodo de matrícula está cerrado.";
                return RedirectToAction(nameof(Matricular));
            }
            if (cursoIds == null || cursoIds.Length == 0)
            {
                TempData["Error"] = "Seleccione al menos un curso.";
                return RedirectToAction(nameof(Matricular));
            }
            return View(await Validar(est, periodo, cursoIds));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(int[] cursoIds)
        {
            var est = await ObtenerEstudiante();
            var periodo = await PeriodoVigente();
            if (est == null || periodo == null) return NotFound();
            if (!periodo.Abierto)
            {
                TempData["Error"] = "El periodo de matrícula está cerrado.";
                return RedirectToAction(nameof(Matricular));
            }
            var vm = await Validar(est, periodo, cursoIds);
            if (vm.Cursos.Count == 0 || !vm.Valido)
                return View("Resumen", vm);

            foreach (var c in vm.Cursos)
                _db.Matriculas.Add(new Matricula { EstudianteId = est.Id, CursoId = c.Id, PeriodoId = periodo.Id, Fecha = DateTime.Now });
            try { await _db.SaveChangesAsync(); }
            catch (DbUpdateException)
            {
                TempData["Error"] = "No se pudo completar la matrícula (curso duplicado). Intente de nuevo.";
                return RedirectToAction(nameof(Matricular));
            }
            TempData["Ok"] = "Matrícula confirmada correctamente. Ya puedes descargar tu comprobante.";
            return RedirectToAction(nameof(MiMatricula));
        }

        // HU-E08: consulta de matrícula
        public async Task<IActionResult> MiMatricula()
        {
            var est = await ObtenerEstudiante();
            var periodo = await PeriodoVigente();
            if (est == null || periodo == null) return NotFound();
            ViewBag.Periodo = periodo;
            return View(await MatriculasActuales(est.Id, periodo.Id));
        }

        // HU-E09: retiro de un curso (libera el cupo y recalcula créditos)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Retirar(int id)
        {
            var est = await ObtenerEstudiante();
            var periodo = await PeriodoVigente();
            if (est == null || periodo == null) return NotFound();
            if (!periodo.Abierto)
            {
                TempData["Error"] = "El periodo de matrícula está cerrado: no se pueden retirar cursos.";
                return RedirectToAction(nameof(MiMatricula));
            }
            var m = await _db.Matriculas.FirstOrDefaultAsync(x => x.Id == id && x.EstudianteId == est.Id && x.PeriodoId == periodo.Id);
            if (m == null) return NotFound();
            _db.Matriculas.Remove(m);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Curso retirado. El cupo fue liberado.";
            return RedirectToAction(nameof(MiMatricula));
        }

        // HU-E08: comprobante en PDF
        public async Task<IActionResult> Comprobante()
        {
            var est = await ObtenerEstudiante();
            var periodo = await PeriodoVigente();
            if (est == null || periodo == null) return NotFound();
            var mats = await MatriculasActuales(est.Id, periodo.Id);
            if (mats.Count == 0)
            {
                TempData["Error"] = "No tiene cursos matriculados para generar el comprobante.";
                return RedirectToAction(nameof(MiMatricula));
            }
            var lineas = new List<string>
            {
                $"Nombre: {est.NombreCompleto}",
                $"Carné: {est.Carne}",
                $"Carrera: {est.Carrera.Nombre}",
                $"Periodo: {periodo.Nombre}",
                $"Fecha de matrícula: {mats.Max(m => m.Fecha):dd/MM/yyyy}",
                "",
                "Cursos matriculados:"
            };
            lineas.AddRange(mats.Select(m => $"  {m.Curso.Codigo} - {m.Curso.Nombre} ({m.Curso.Creditos} cr.) - {m.Curso.Horario}"));
            lineas.Add("");
            lineas.Add($"Total de créditos: {mats.Sum(m => m.Curso.Creditos)}");

            var pdf = PdfSimple.Crear("Comprobante de matrícula", lineas);
            return File(pdf, "application/pdf", $"Comprobante_{est.Carne}.pdf");
        }
    }
}
