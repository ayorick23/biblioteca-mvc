using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services;

public interface ILibroService
{
    List<Libro> ObtenerLibros();
    Libro? ObtenerLibroPorId(int id);
    void CrearLibro(Libro libro);
    void ActualizarLibro(Libro libro);
    void EliminarLibro(int id);
}
