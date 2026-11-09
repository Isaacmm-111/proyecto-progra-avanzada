using Microsoft.AspNetCore.Identity;
using ProyectoFinal_MonteroIsaac.Models;

namespace ProyectoFinal_MonteroIsaac.Data
{
    public static class SeedData
    {
        public static readonly string[] Roles = { "Administrador", "Docente", "Estudiante" };
        public const string AdminEmail = "admin@fidelitas.ac.cr";
        public const string AdminPassword = "Admin123";
        public const string DocentePassword = "Docente1";
        public const string EstudiantePassword = "Estudiante1";

        private static async Task CrearUsuario(UserManager<ApplicationUser> um, string email, string nombre, string password, string rol)
        {
            if (await um.FindByEmailAsync(email) != null) return;
            var u = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, NombreCompleto = nombre };
            var r = await um.CreateAsync(u, password);
            if (r.Succeeded) await um.AddToRoleAsync(u, rol);
        }

        public static async Task InitializeAsync(IServiceProvider sp)
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var um = sp.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in Roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            await CrearUsuario(um, AdminEmail, "Administrador del Sistema", AdminPassword, "Administrador");
            await CrearUsuario(um, "solano@correo.edu", "Prof. Solano Mora", DocentePassword, "Docente");
            await CrearUsuario(um, "ana.mora@correo.edu", "Ana Mora Solís", EstudiantePassword, "Estudiante");

            if (!db.Carreras.Any())
            {
                var sis = new Carrera { Codigo = "SIS", Nombre = "Ingeniería en Sistemas", DuracionAnios = 4, Activa = true };
                var adm = new Carrera { Codigo = "ADM", Nombre = "Administración de Empresas", DuracionAnios = 4, Activa = true };
                var con = new Carrera { Codigo = "CON", Nombre = "Contaduría", DuracionAnios = 4, Activa = true };
                var der = new Carrera { Codigo = "DER", Nombre = "Derecho", DuracionAnios = 5, Activa = true };
                var psi = new Carrera { Codigo = "PSI", Nombre = "Psicología", DuracionAnios = 5, Activa = false };
                db.Carreras.AddRange(sis, adm, con, der, psi);

                var solano = new Docente { Nombre = "Prof. Solano Mora", Correo = "solano@correo.edu", Especialidad = "Estructuras de datos" };
                var araya = new Docente { Nombre = "Prof. Araya Vega", Correo = "araya@correo.edu", Especialidad = "Bases de datos" };
                var chaves = new Docente { Nombre = "Prof. Chaves Rojas", Correo = "chaves@correo.edu", Especialidad = "Matemática" };
                var mena = new Docente { Nombre = "Prof. Mena Soto", Correo = "mena@correo.edu", Especialidad = "Redes" };
                var vargas = new Docente { Nombre = "Prof. Vargas Díaz", Correo = "vargas@correo.edu", Especialidad = "Programación" };
                var quiros = new Docente { Nombre = "Prof. Quirós Mata", Correo = "quiros@correo.edu", Especialidad = "Contabilidad" };
                var brown = new Docente { Nombre = "Prof. Brown", Correo = "brown@correo.edu", Especialidad = "Inglés técnico" };
                db.Docentes.AddRange(solano, araya, chaves, mena, vargas, quiros, brown);
                await db.SaveChangesAsync();

                var cursos = new[]
                {
                    new Curso { Codigo = "SIS-101", Nombre = "Programación I", Creditos = 4, Cuatrimestre = 1, CarreraId = sis.Id, DocenteId = vargas.Id, Horario = "Lun y Mié 18:00-20:00", CupoMaximo = 30 },
                    new Curso { Codigo = "MAT-120", Nombre = "Cálculo Diferencial", Creditos = 4, Cuatrimestre = 2, CarreraId = sis.Id, DocenteId = chaves.Id, Horario = "Mar y Jue 18:00-20:00", CupoMaximo = 30 },
                    new Curso { Codigo = "ING-110", Nombre = "Inglés Técnico", Creditos = 4, Cuatrimestre = 2, CarreraId = sis.Id, DocenteId = brown.Id, Horario = "Vie 18:00-21:30", CupoMaximo = 30 },
                    new Curso { Codigo = "SIS-201", Nombre = "Estructuras de Datos", Creditos = 4, Cuatrimestre = 3, CarreraId = sis.Id, DocenteId = solano.Id, Horario = "Lun y Mié 14:00-16:00", CupoMaximo = 30 },
                    new Curso { Codigo = "SIS-210", Nombre = "Bases de Datos I", Creditos = 4, Cuatrimestre = 3, CarreraId = sis.Id, DocenteId = araya.Id, Horario = "Mar y Jue 14:00-16:00", CupoMaximo = 25 },
                    new Curso { Codigo = "SIS-220", Nombre = "Redes", Creditos = 3, Cuatrimestre = 4, CarreraId = sis.Id, DocenteId = mena.Id, Horario = "Mié 18:00-21:00", CupoMaximo = 1 },
                    new Curso { Codigo = "MAT-210", Nombre = "Álgebra Lineal", Creditos = 3, Cuatrimestre = 3, CarreraId = sis.Id, DocenteId = chaves.Id, Horario = "Vie 14:00-17:00", CupoMaximo = 30 },
                    new Curso { Codigo = "ADM-110", Nombre = "Contabilidad General", Creditos = 3, Cuatrimestre = 1, CarreraId = adm.Id, DocenteId = quiros.Id, Horario = "Vie 18:00-21:00", CupoMaximo = 35 },
                    new Curso { Codigo = "CON-101", Nombre = "Contabilidad Financiera", Creditos = 4, Cuatrimestre = 1, CarreraId = con.Id, DocenteId = quiros.Id, Horario = "Sáb 08:00-12:00", CupoMaximo = 35 },
                    new Curso { Codigo = "DER-101", Nombre = "Introducción al Derecho", Creditos = 4, Cuatrimestre = 1, CarreraId = der.Id, Horario = "Jue 18:00-22:00", CupoMaximo = 40 }
                };
                db.Cursos.AddRange(cursos);

                db.Periodos.Add(new Periodo
                {
                    Nombre = "II Cuatrimestre 2026",
                    FechaApertura = new DateTime(2026, 8, 3),
                    FechaCierre = new DateTime(2026, 8, 28),
                    Abierto = true,
                    LimiteCreditos = 18
                });
                await db.SaveChangesAsync();

                var ana = await um.FindByEmailAsync("ana.mora@correo.edu");
                var estAna = new Estudiante { UserId = ana.Id, Carne = "2026-0141", NombreCompleto = "Ana Mora Solís", Correo = ana.Email, CarreraId = sis.Id, Activo = true };
                var diego = new Estudiante { Carne = "2026-0156", NombreCompleto = "Diego Castro Ruiz", Correo = "diego.castro@correo.edu", CarreraId = sis.Id, Activo = true };
                var luis = new Estudiante { Carne = "2026-0187", NombreCompleto = "Luis Pérez Ríos", Correo = "luis.perez@correo.edu", CarreraId = adm.Id, Activo = true };
                var maria = new Estudiante { Carne = "2025-0932", NombreCompleto = "María Núñez Vega", Correo = "maria.nunez@correo.edu", CarreraId = con.Id, Activo = false };
                var carlos = new Estudiante { Carne = "2026-0203", NombreCompleto = "Carlos Soto Mora", Correo = "carlos.soto@correo.edu", CarreraId = der.Id, Activo = true };
                db.Estudiantes.AddRange(estAna, diego, luis, maria, carlos);
                await db.SaveChangesAsync();

                var p = db.Periodos.First();
                Curso C(string cod) => cursos.First(x => x.Codigo == cod);
                var fecha = new DateTime(2026, 8, 12);
                db.Matriculas.AddRange(
                    new Matricula { EstudianteId = estAna.Id, CursoId = C("SIS-101").Id, PeriodoId = p.Id, Fecha = fecha },
                    new Matricula { EstudianteId = estAna.Id, CursoId = C("MAT-120").Id, PeriodoId = p.Id, Fecha = fecha },
                    new Matricula { EstudianteId = estAna.Id, CursoId = C("ING-110").Id, PeriodoId = p.Id, Fecha = fecha },
                    new Matricula { EstudianteId = diego.Id, CursoId = C("SIS-220").Id, PeriodoId = p.Id, Fecha = fecha },
                    new Matricula { EstudianteId = diego.Id, CursoId = C("SIS-201").Id, PeriodoId = p.Id, Fecha = fecha },
                    new Matricula { EstudianteId = luis.Id, CursoId = C("ADM-110").Id, PeriodoId = p.Id, Fecha = new DateTime(2026, 8, 13) },
                    new Matricula { EstudianteId = carlos.Id, CursoId = C("DER-101").Id, PeriodoId = p.Id, Fecha = new DateTime(2026, 8, 14) });
                await db.SaveChangesAsync();
            }
        }
    }
}
