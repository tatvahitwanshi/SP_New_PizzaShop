using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc;
using PizzashopRMS.Models;

namespace PizzashopRMS.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

     // Constructor to initialize the logger
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // Redirects to the login page
    public IActionResult Index()
    {
        return RedirectToAction("LoginView","Login");
    }

     // Displays the privacy policy page
    public IActionResult Privacy()
    {
        return View();
    }

     // Handles errors and displays an error view
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
