using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_MonteroIsaac.Models
{
    public class Curso
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(10)]
        [Display(Name = "Código")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(120)]
        public string Nombre { get; set; }

        [Range(1, 6, ErrorMessage = "Los créditos deben estar entre 1 y 6.")]
        [Display(Name = "Créditos")]
        public int Creditos { get; set; }

        [Range(1, 20, ErrorMessage = "El cuatrimestre debe estar entre 1 y 20.")]
        public int Cuatrimestre { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una carrera.")]
        [Display(Name = "Carrera")]
        public int CarreraId { get; set; }
        public Carrera Carrera { get; set; }

        [Display(Name = "Docente")]
        public int? DocenteId { get; set; }
        public Docente Docente { get; set; }

        [Required(ErrorMessage = "El horario es obligatorio.")]
        [RegularExpression(Services.HorarioHelper.Patron, ErrorMessage = "Formato inválido. Ejemplo: Lun y Mié 14:00-16:00")]
        [StringLength(60)]
        [Display(Name = "Horario (ej. Lun y Mié 14:00-16:00)")]
        public string Horario { get; set; }

        [Range(1, 500, ErrorMessage = "El cupo máximo debe ser mayor que cero.")]
        [Display(Name = "Cupo máximo")]
        public int CupoMaximo { get; set; } = 30;

        public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int CuposDisponibles => CupoMaximo - (Matriculas?.Count ?? 0);
    }
}
