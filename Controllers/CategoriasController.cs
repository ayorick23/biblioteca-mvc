using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaMVC.Models;
using BibliotecaMVC.Services;

namespace BibliotecaMVC.Controllers;

[Authorize]
public class CategoriasController : Controller
{
    private readonly ICategoriaService _categoriaService;
    // Semana 15 - Actividad 1: logger para registrar los eventos del módulo Categorías.
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(ICategoriaService categoriaService, ILogger<CategoriasController> logger)
    {
        _categoriaService = categoriaService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Consultando el listado de categorías.");

        try
        {
            var categorias = _categoriaService.ObtenerCategorias();
            return View(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Se produjo un error al consultar el listado de categorías.");
            // Se relanza para que la página de error personalizada se muestre al usuario.
            throw;
        }
    }

    public IActionResult Crear()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(Categoria categoria)
    {
        _logger.LogInformation("Iniciando la creación de una categoría.");

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Los datos de la categoría no son válidos.");
            return View(categoria);
        }

        _categoriaService.CrearCategoria(categoria);
        _logger.LogInformation("Categoría {Nombre} creada correctamente.", categoria.Nombre);
        TempData["Mensaje"] = "La categoría se agregó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Editar(int id)
    {
        _logger.LogInformation("Consultando la categoría con Id {Id} para editar.", id);

        var categoria = _categoriaService.ObtenerCategoriaPorId(id);
        if (categoria == null)
        {
            _logger.LogWarning("No se encontró la categoría con Id {Id} para editar.", id);
            return NotFound();
        }

        return View(categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(Categoria categoria)
    {
        _logger.LogInformation("Iniciando la edición de la categoría con Id {Id}.", categoria.Id);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Los datos de la categoría con Id {Id} no son válidos.", categoria.Id);
            return View(categoria);
        }

        _categoriaService.ActualizarCategoria(categoria);
        _logger.LogInformation("Categoría con Id {Id} actualizada correctamente.", categoria.Id);
        TempData["Mensaje"] = "La categoría se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // La confirmación se solicita con SweetAlert desde el listado.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        _logger.LogInformation("Iniciando la eliminación de la categoría con Id {Id}.", id);

        _categoriaService.EliminarCategoria(id);
        _logger.LogInformation("Categoría con Id {Id} eliminada correctamente.", id);
        TempData["Mensaje"] = "La categoría se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
