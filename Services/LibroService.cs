using BibliotecaMVC.Data;
using BibliotecaMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Services;

// CRUD completo de Libros (Mostrar, Agregar, Editar y Eliminar) usando
// Entity Framework Core a través del DbContext (BibliotecaContext).
public class LibroService : ILibroService
{
    private readonly BibliotecaContext _context;
    private readonly ICategoriaService _categoriaService;

    public LibroService(BibliotecaContext context, ICategoriaService categoriaService)
    {
        _context = context;
        _categoriaService = categoriaService;
    }

    // Como CategoriaNombre no está mapeado en la base de datos, se completa
    // aquí a partir de las categorías ya existentes, para mostrarlo en la vista.
    private void CompletarNombreCategoria(Libro libro)
    {
        var categoria = _categoriaService.ObtenerCategoriaPorId(libro.CategoriaId);
        libro.CategoriaNombre = categoria?.Nombre ?? string.Empty;
    }

    public List<Libro> ObtenerLibros()
    {
        var libros = _context.Libros
            .AsNoTracking()
            .OrderBy(l => l.Titulo)
            .ToList();

        foreach (var libro in libros)
        {
            CompletarNombreCategoria(libro);
        }

        return libros;
    }

    public Libro? ObtenerLibroPorId(int id)
    {
        var libro = _context.Libros
            .AsNoTracking()
            .FirstOrDefault(l => l.Id == id);

        if (libro != null)
        {
            CompletarNombreCategoria(libro);
        }

        return libro;
    }

    public void CrearLibro(Libro libro)
    {
        _context.Libros.Add(libro);
        _context.SaveChanges();
    }

    // Actividad 1: Editar libro con Find(), Update() y SaveChanges().
    public void ActualizarLibro(Libro libro)
    {
        // Find(): localiza el libro en la base de datos por su Id.
        var libroExistente = _context.Libros.Find(libro.Id);
        if (libroExistente == null)
        {
            return;
        }

        libroExistente.Titulo = libro.Titulo;
        libroExistente.Autor = libro.Autor;
        libroExistente.CategoriaId = libro.CategoriaId;
        libroExistente.Anio = libro.Anio;

        // Update(): marca el libro como modificado.
        _context.Libros.Update(libroExistente);
        // SaveChanges(): guarda los cambios en SQL Server.
        _context.SaveChanges();
    }

    // Actividad 2: Eliminar libro con Find(), Remove() y SaveChanges().
    public void EliminarLibro(int id)
    {
        // Find(): localiza el libro en la base de datos por su Id.
        var libroExistente = _context.Libros.Find(id);
        if (libroExistente == null)
        {
            return;
        }

        // Remove(): marca el libro para ser eliminado.
        _context.Libros.Remove(libroExistente);
        // SaveChanges(): confirma la eliminación en SQL Server.
        _context.SaveChanges();
    }
}
