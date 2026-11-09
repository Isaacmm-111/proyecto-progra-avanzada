using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_MonteroIsaac.Models
{
    public class Periodo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio."), StringLength(60)]
        public string Nombre { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de apertura")]
        public DateTime FechaApertura { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de cierre")]
        public DateTime FechaCierre { get; set; }

        public bool Abierto { get; set; }

        [Range(1, 30, ErrorMessage = "El límite debe estar entre 1 y 30 créditos.")]
        [Display(Name = "Límite de créditos")]
        public int LimiteCreditos { get; set; } = 18;
    }
}
