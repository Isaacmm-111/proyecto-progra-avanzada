using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_MonteroIsaac.Models
{
    public class Carrera
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(10)]
        [Display(Name = "Código")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(120)]
        public string Nombre { get; set; }

        [Range(1, 10, ErrorMessage = "La duración debe estar entre 1 y 10 años.")]
        [Display(Name = "Duración (años)")]
        public int DuracionAnios { get; set; }

        public bool Activa { get; set; } = true;

        public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
        public ICollection<Estudiante> Estudiantes { get; set; } = new List<Estudiante>();
    }
}
