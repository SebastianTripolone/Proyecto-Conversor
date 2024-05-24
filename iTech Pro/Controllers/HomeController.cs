using iTech_Pro.Filters;
using iTech_Pro.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace iTech_Pro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

       

        [AuthorizeUser]
        public IActionResult Index()
        {

            return View();
        }

        [AuthorizeUser]
        public IActionResult Privacy()
        {
            return View();
        }

        [AuthorizeUser]
        public IActionResult Historia()
        {
            return View();
        }

        [AuthorizeUser]
        public IActionResult Graficos()
        {
            return View();
        }

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Remove("usuario");
            return RedirectToAction("Login", "Acceso");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}