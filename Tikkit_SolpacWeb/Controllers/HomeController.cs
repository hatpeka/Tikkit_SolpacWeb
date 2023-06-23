using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Tikkit_SolpacWeb.Models;
using Microsoft.AspNetCore.Session;

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
            ViewBag.UserName = TempData["UserName"];
            return View();
        }
        public IActionResult Staff()
        {
            ViewBag.UserName = TempData["UserName"];
            return View();
        }
        public IActionResult Client()
        {
            ViewBag.UserName = TempData["UserName"];
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