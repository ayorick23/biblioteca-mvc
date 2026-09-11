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
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Eliminar(int id)
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
    public IActionResult EliminarConfirmado(int id)
    {
        _categoriaService.EliminarCategoria(id);
        return RedirectToAction(nameof(Index));
    }
}
