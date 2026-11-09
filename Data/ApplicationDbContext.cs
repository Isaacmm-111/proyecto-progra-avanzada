using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_MonteroIsaac.Models;

namespace ProyectoFinal_MonteroIsaac.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Carrera> Carreras { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Docente> Docentes { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }
        public DbSet<Periodo> Periodos { get; set; }

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<Carrera>().HasIndex(c => c.Codigo).IsUnique();
            b.Entity<Curso>().HasIndex(c => c.Codigo).IsUnique();
            b.Entity<Estudiante>().HasIndex(e => e.Carne).IsUnique();
            b.Entity<Estudiante>().HasIndex(e => e.UserId);

            b.Entity<Curso>().Ignore(c => c.CuposDisponibles);

            b.Entity<Curso>().HasOne(c => c.Carrera).WithMany(c => c.Cursos)
                .HasForeignKey(c => c.CarreraId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<Curso>().HasOne(c => c.Docente).WithMany(d => d.Cursos)
                .HasForeignKey(c => c.DocenteId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<Estudiante>().HasOne(e => e.Carrera).WithMany(c => c.Estudiantes)
                .HasForeignKey(e => e.CarreraId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<Estudiante>().HasOne(e => e.User).WithMany()
                .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);

            b.Entity<Matricula>().HasOne(m => m.Estudiante).WithMany(e => e.Matriculas)
                .HasForeignKey(m => m.EstudianteId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<Matricula>().HasOne(m => m.Curso).WithMany(c => c.Matriculas)
                .HasForeignKey(m => m.CursoId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<Matricula>().HasOne(m => m.Periodo).WithMany()
                .HasForeignKey(m => m.PeriodoId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<Matricula>().HasIndex(m => new { m.EstudianteId, m.CursoId, m.PeriodoId }).IsUnique();
        }
    }
}
