using ProyectoFinal_MonteroIsaac.Models;

namespace ProyectoFinal_MonteroIsaac.ViewModels
{
    public class EstudianteHomeViewModel
    {
        public string NombreCompleto { get; set; }
        public string Carne { get; set; }
        public string Carrera { get; set; }
        public string PeriodoVigente { get; set; }
        public bool PeriodoAbierto { get; set; }
        public int CreditosMatriculados { get; set; }
        public int LimiteCreditos { get; set; }
        public List<Matricula> Matriculas { get; set; } = new();
    }

    public class ValidacionItem
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }
    }

    public class ResumenMatriculaViewModel
    {
        public string Periodo { get; set; }
        public List<Curso> Cursos { get; set; } = new();
        public int CreditosSeleccionados { get; set; }
        public int CreditosAcumulados { get; set; }
        public int LimiteCreditos { get; set; }
        public List<ValidacionItem> Validaciones { get; set; } = new();
        public bool Valido => Validaciones.All(v => v.Ok);
    }

    public class CatalogoViewModel
    {
        public string Carrera { get; set; }
        public string Periodo { get; set; }
        public bool PeriodoAbierto { get; set; }
        public List<Curso> Cursos { get; set; } = new();
        public HashSet<int> Matriculados { get; set; } = new();
        public string Busqueda { get; set; }
        public int? Cuatrimestre { get; set; }
        public List<int> Cuatrimestres { get; set; } = new();
    }
}
