using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal_MonteroIsaac.Models;

namespace ProyectoFinal_MonteroIsaac.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Administrador")) return RedirectToAction("Index", "Admin");
                if (User.IsInRole("Estudiante")) return RedirectToAction("Index", "Estudiante");
                if (User.IsInRole("Docente")) return RedirectToAction("Index", "Docente");
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
