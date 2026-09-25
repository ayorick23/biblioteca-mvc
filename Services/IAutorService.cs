using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services;

public interface IAutorService
{
    List<Autor> ObtenerAutores();
    Autor? ObtenerAutorPorId(int id);
    void CrearAutor(Autor autor);
    void ActualizarAutor(Autor autor);
    void EliminarAutor(int id);
}
