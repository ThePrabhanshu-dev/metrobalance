using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MetroBalance.Web.Models;

namespace MetroBalance.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Faq()
    {
        return View();
    }

    /// <summary>Step by step guide for reading the balance of a real DMRC Smart Card.</summary>
    public IActionResult Guide()
    {
        return View();
    }

    /// <summary>Presentation deck used to showcase the project to DMRC and other stakeholders.</summary>
    public IActionResult Showcase()
    {
        ViewData["Title"] = "Showcase";
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
