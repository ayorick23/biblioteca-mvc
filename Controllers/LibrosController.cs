using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaMVC.Models;
using BibliotecaMVC.Services;

namespace BibliotecaMVC.Controllers;

// Semana 13: los tres roles pueden consultar; solo Admin y Bibliotecario pueden agregar, editar y eliminar.
[Authorize(Roles = "Admin,Bibliotecario,Usuario")]
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

    // Semana 13: consulta del detalle de un libro (los tres roles).
    public IActionResult Detalle(int id)
    {
        var libro = _libroService.ObtenerLibroPorId(id);
        if (libro == null)
        {
            return NotFound();
        }

        return View(libro);
    }

    [Authorize(Roles = "Admin,Bibliotecario")]
    public IActionResult Crear()
    {
        CargarCategorias();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Bibliotecario")]
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

    [Authorize(Roles = "Admin,Bibliotecario")]
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
    [Authorize(Roles = "Admin,Bibliotecario")]
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
    [Authorize(Roles = "Admin,Bibliotecario")]
    public IActionResult Eliminar(int id)
    {
        _libroService.EliminarLibro(id);
        TempData["Mensaje"] = "El libro se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
