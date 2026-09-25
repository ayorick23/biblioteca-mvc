using BibliotecaMVC.Data;
using BibliotecaMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Services;

// CRUD completo de Autores (Mostrar, Agregar, Editar y Eliminar) usando
// Entity Framework Core a través del DbContext (BibliotecaContext).
public class AutorService : IAutorService
{
    private readonly BibliotecaContext _context;

    public AutorService(BibliotecaContext context)
    {
        _context = context;
    }

    public List<Autor> ObtenerAutores()
    {
        return _context.Autores
            .AsNoTracking()
            .OrderBy(a => a.Apellidos)
            .ToList();
    }

    public Autor? ObtenerAutorPorId(int id)
    {
        return _context.Autores
            .AsNoTracking()
            .FirstOrDefault(a => a.Id == id);
    }

    public void CrearAutor(Autor autor)
    {
        _context.Autores.Add(autor);
        _context.SaveChanges();
    }

    public void ActualizarAutor(Autor autor)
    {
        // Find(): localiza el autor en la base de datos por su Id.
        var autorExistente = _context.Autores.Find(autor.Id);
        if (autorExistente == null)
        {
            return;
        }

        autorExistente.Nombres = autor.Nombres;
        autorExistente.Apellidos = autor.Apellidos;
        autorExistente.Nacionalidad = autor.Nacionalidad;

        // Update(): marca el autor como modificado.
        _context.Autores.Update(autorExistente);
        // SaveChanges(): guarda los cambios en SQL Server.
        _context.SaveChanges();
    }

    public void EliminarAutor(int id)
    {
        // Find(): localiza el autor en la base de datos por su Id.
        var autorExistente = _context.Autores.Find(id);
        if (autorExistente == null)
        {
            return;
        }

        // Remove(): marca el autor para ser eliminado.
        _context.Autores.Remove(autorExistente);
        // SaveChanges(): confirma la eliminación en SQL Server.
        _context.SaveChanges();
    }
}
