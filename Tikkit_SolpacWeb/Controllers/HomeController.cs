using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Tikkit_SolpacWeb.Models;

namespace Tikkit_SolpacWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Admin()
        {
            return View();
        }
        public IActionResult Staff()
        {
            return View();
        }
        public IActionResult Client()
        {
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}