using Microsoft.AspNetCore.Mvc;
using BibliotecaMVC.Models;
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

    public IActionResult Crear()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(Autor autor)
    {
        if (!ModelState.IsValid)
        {
            return View(autor);
        }

        _autorService.CrearAutor(autor);
        TempData["Mensaje"] = "El autor se agregó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Editar(int id)
    {
        var autor = _autorService.ObtenerAutorPorId(id);
        if (autor == null)
        {
            return NotFound();
        }

        return View(autor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(Autor autor)
    {
        if (!ModelState.IsValid)
        {
            return View(autor);
        }

        _autorService.ActualizarAutor(autor);
        TempData["Mensaje"] = "El autor se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // La confirmación se solicita con SweetAlert desde el listado.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        _autorService.EliminarAutor(id);
        TempData["Mensaje"] = "El autor se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
