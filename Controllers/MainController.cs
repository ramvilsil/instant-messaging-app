using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Application.ViewModels;

namespace Application.Controllers;

public class MainController : Controller
{
    private readonly ILogger<MainController> _logger;

    public MainController
    (
        ILogger<MainController> logger
    )
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var response = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };

        return View(response);
    }
}