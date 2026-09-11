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
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Eliminar(int id)
    {
        var libro = _libroService.ObtenerLibroPorId(id);
        if (libro == null)
        {
            return NotFound();
        }

        return View(libro);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EliminarConfirmado(int id)
    {
        _libroService.EliminarLibro(id);
        return RedirectToAction(nameof(Index));
    }
}
