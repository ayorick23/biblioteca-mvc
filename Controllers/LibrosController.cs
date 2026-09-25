using Microsoft.AspNetCore.Mvc;
using BibliotecaMVC.Models;
using BibliotecaMVC.Services;

namespace BibliotecaMVC.Controllers;

public class LibrosController : Controller
{
    private readonly ILibroService _libroService;
    private readonly ICategoriaService _categoriaService;

    public LibrosController(ILibroService libroService, ICategoriaService categoriaService)
    {
        _libroService = libroService;
        _categoriaService = categoriaService;
    }

    private void CargarCategorias()
    {
        ViewBag.Categorias = _categoriaService.ObtenerCategorias();
    }

    public IActionResult Index()
    {
        var libros = _libroService.ObtenerLibros();
        return View(libros);
    }

    public IActionResult Crear()
    {
        CargarCategorias();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(Libro libro)
    {
        if (!ModelState.IsValid)
        {
            CargarCategorias();
            return View(libro);
        }

        _libroService.CrearLibro(libro);
        TempData["Mensaje"] = "El libro se agregó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Editar(int id)
    {
        var libro = _libroService.ObtenerLibroPorId(id);
        if (libro == null)
        {
            return NotFound();
        }

        CargarCategorias();
        return View(libro);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(Libro libro)
    {
        if (!ModelState.IsValid)
        {
            CargarCategorias();
            return View(libro);
        }

        _libroService.ActualizarLibro(libro);
        TempData["Mensaje"] = "El libro se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // La confirmación se solicita con SweetAlert desde el listado.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        _libroService.EliminarLibro(id);
        TempData["Mensaje"] = "El libro se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
