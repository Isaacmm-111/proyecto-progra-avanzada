using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_MonteroIsaac.Models
{
    public class Docente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(120)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        [StringLength(150)]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La especialidad es obligatoria.")]
        [StringLength(100)]
        public string Especialidad { get; set; }

        public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
    }
}
