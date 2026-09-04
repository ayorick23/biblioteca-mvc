using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services;

public interface IAutorService
{
    List<Autor> ObtenerAutores();
    Autor? ObtenerAutorPorId(int id);
}
