using Microsoft.AspNetCore.Identity;

namespace ProyectoFinal_MonteroIsaac.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string NombreCompleto { get; set; }
        public bool Activo { get; set; } = true;
    }
}
