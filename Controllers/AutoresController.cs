using Microsoft.AspNetCore.Mvc;
using BibliotecaMVC.Services;

namespace BibliotecaMVC.Controllers;

public class AutoresController : Controller
{
    private readonly IAutorService _autorService;

    public AutoresController(IAutorService autorService)
    {
        _autorService = autorService;
    }

    public IActionResult Index()
    {
        var autores = _autorService.ObtenerAutores();
        return View(autores);
    }

    public IActionResult Detalle(int id)
    {
        var autor = _autorService.ObtenerAutorPorId(id);
        if (autor == null)
        {
            return NotFound();
        }

        return View(autor);
    }
}
