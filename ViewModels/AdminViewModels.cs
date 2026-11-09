namespace ProyectoFinal_MonteroIsaac.ViewModels
{
    public class BarraItem
    {
        public string Etiqueta { get; set; }
        public int Valor { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalEstudiantes { get; set; }
        public int TotalCarreras { get; set; }
        public int TotalCursos { get; set; }
        public int TotalDocentes { get; set; }
        public string PeriodoVigente { get; set; }
        public List<BarraItem> MatriculasPorCarrera { get; set; } = new();
    }

    public class UsuarioRolViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }
    }

    public class ConfiguracionViewModel
    {
        public Models.Periodo Periodo { get; set; }
        public List<UsuarioRolViewModel> Usuarios { get; set; } = new();
    }
}
