using Microsoft.AspNetCore.Mvc;

namespace DirectNet.SampleApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}