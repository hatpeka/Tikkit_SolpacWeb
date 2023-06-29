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

        [RequireLogin]
        public IActionResult Admin()
        {
            string? userName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = userName;
            return View();
        }
        [RequireLogin]
        public IActionResult Staff()
        {
            string? userName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = userName;
            return View();
        }
        [RequireLogin]

        public IActionResult Client()
        {
            string? userName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = userName;
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