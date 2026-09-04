using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services;

public class AutorServiceAlterno : IAutorService
{
    private readonly List<Autor> _autores = new()
    {
        new Autor { Id = 1, Nombres = "Miguel", Apellidos = "de Cervantes", Nacionalidad = "España" },
        new Autor { Id = 2, Nombres = "Jane", Apellidos = "Austen", Nacionalidad = "Reino Unido" },
        new Autor { Id = 3, Nombres = "Franz", Apellidos = "Kafka", Nacionalidad = "Chequia" },
    };

    public List<Autor> ObtenerAutores()
    {
        return _autores;
    }

    public Autor? ObtenerAutorPorId(int id)
    {
        return _autores.FirstOrDefault(a => a.Id == id);
    }
}
