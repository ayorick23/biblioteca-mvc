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
    // Semana 15 - Actividad 1: logger para registrar los eventos del módulo Libros.
    private readonly ILogger<LibrosController> _logger;

    public LibrosController(ILibroService libroService, ICategoriaService categoriaService, ILogger<LibrosController> logger)
    {
        _libroService = libroService;
        _categoriaService = categoriaService;
        _logger = logger;
    }

    private void CargarCategorias()
    {
        ViewBag.Categorias = _categoriaService.ObtenerCategorias();
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Consultando el listado de libros.");

        try
        {
            var libros = _libroService.ObtenerLibros();
            return View(libros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Se produjo un error al consultar el listado de libros.");
            // Se relanza para que la página de error personalizada se muestre al usuario.
            throw;
        }
    }

    // Semana 13: consulta del detalle de un libro (los tres roles).
    public IActionResult Detalle(int id)
    {
        _logger.LogInformation("Consultando el detalle del libro con Id {Id}.", id);

        var libro = _libroService.ObtenerLibroPorId(id);
        if (libro == null)
        {
            _logger.LogWarning("No se encontró el libro con Id {Id}.", id);
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
        _logger.LogInformation("Iniciando la creación de un libro.");

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Los datos del libro no son válidos.");
            CargarCategorias();
            return View(libro);
        }

        _libroService.CrearLibro(libro);
        _logger.LogInformation("Libro {Titulo} creado correctamente.", libro.Titulo);
        TempData["Mensaje"] = "El libro se agregó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Bibliotecario")]
    public IActionResult Editar(int id)
    {
        _logger.LogInformation("Consultando el libro con Id {Id} para editar.", id);

        var libro = _libroService.ObtenerLibroPorId(id);
        if (libro == null)
        {
            _logger.LogWarning("No se encontró el libro con Id {Id} para editar.", id);
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
        _logger.LogInformation("Iniciando la edición del libro con Id {Id}.", libro.Id);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Los datos del libro con Id {Id} no son válidos.", libro.Id);
            CargarCategorias();
            return View(libro);
        }

        _libroService.ActualizarLibro(libro);
        _logger.LogInformation("Libro con Id {Id} actualizado correctamente.", libro.Id);
        TempData["Mensaje"] = "El libro se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // La confirmación se solicita con SweetAlert desde el listado.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Bibliotecario")]
    public IActionResult Eliminar(int id)
    {
        _logger.LogInformation("Iniciando la eliminación del libro con Id {Id}.", id);

        _libroService.EliminarLibro(id);
        _logger.LogInformation("Libro con Id {Id} eliminado correctamente.", id);
        TempData["Mensaje"] = "El libro se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
