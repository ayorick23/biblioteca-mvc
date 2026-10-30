using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaMVC.Models;
using BibliotecaMVC.Services;

namespace BibliotecaMVC.Controllers;

[Authorize]
public class AutoresController : Controller
{
    private readonly IAutorService _autorService;
    // Semana 15: logger para registrar los eventos del módulo Autores.
    private readonly ILogger<AutoresController> _logger;

    public AutoresController(IAutorService autorService, ILogger<AutoresController> logger)
    {
        _autorService = autorService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Consultando el listado de autores.");

        try
        {
            var autores = _autorService.ObtenerAutores();
            return View(autores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Se produjo un error al consultar el listado de autores.");
            // Se relanza para que la página de error personalizada se muestre al usuario.
            throw;
        }
    }

    public IActionResult Detalle(int id)
    {
        _logger.LogInformation("Consultando el detalle del autor con Id {Id}.", id);

        var autor = _autorService.ObtenerAutorPorId(id);
        if (autor == null)
        {
            _logger.LogWarning("No se encontró el autor con Id {Id}.", id);
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
        _logger.LogInformation("Iniciando la creación de un autor.");

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Los datos del autor no son válidos.");
            return View(autor);
        }

        _autorService.CrearAutor(autor);
        _logger.LogInformation("Autor {Nombres} creado correctamente.", autor.Nombres);
        TempData["Mensaje"] = "El autor se agregó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Editar(int id)
    {
        var autor = _autorService.ObtenerAutorPorId(id);
        if (autor == null)
        {
            _logger.LogWarning("No se encontró el autor con Id {Id} para editar.", id);
            return NotFound();
        }

        return View(autor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(Autor autor)
    {
        _logger.LogInformation("Iniciando la edición del autor con Id {Id}.", autor.Id);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Los datos del autor con Id {Id} no son válidos.", autor.Id);
            return View(autor);
        }

        _autorService.ActualizarAutor(autor);
        _logger.LogInformation("Autor con Id {Id} actualizado correctamente.", autor.Id);
        TempData["Mensaje"] = "El autor se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // La confirmación se solicita con SweetAlert desde el listado.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        _logger.LogInformation("Iniciando la eliminación del autor con Id {Id}.", id);

        _autorService.EliminarAutor(id);
        _logger.LogInformation("Autor con Id {Id} eliminado correctamente.", id);
        TempData["Mensaje"] = "El autor se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
