using Microsoft.AspNetCore.Mvc;
using BibliotecaMVC.Models;
using BibliotecaMVC.Services;

namespace BibliotecaMVC.Controllers;

public class CategoriasController : Controller
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    public IActionResult Index()
    {
        var categorias = _categoriaService.ObtenerCategorias();
        return View(categorias);
    }

    public IActionResult Crear()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(Categoria categoria)
    {
        if (!ModelState.IsValid)
        {
            return View(categoria);
        }

        _categoriaService.CrearCategoria(categoria);
        TempData["Mensaje"] = "La categoría se agregó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Editar(int id)
    {
        var categoria = _categoriaService.ObtenerCategoriaPorId(id);
        if (categoria == null)
        {
            return NotFound();
        }

        return View(categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(Categoria categoria)
    {
        if (!ModelState.IsValid)
        {
            return View(categoria);
        }

        _categoriaService.ActualizarCategoria(categoria);
        TempData["Mensaje"] = "La categoría se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // La confirmación se solicita con SweetAlert desde el listado.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        _categoriaService.EliminarCategoria(id);
        TempData["Mensaje"] = "La categoría se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
