using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services;

public class AutorService : IAutorService
{
    private readonly List<Autor> _autores = new()
    {
        new Autor { Id = 1, Nombres = "Gabriel", Apellidos = "García Márquez", Nacionalidad = "Colombia" },
        new Autor { Id = 2, Nombres = "Isabel", Apellidos = "Allende", Nacionalidad = "Chile" },
        new Autor { Id = 3, Nombres = "Mario", Apellidos = "Vargas Llosa", Nacionalidad = "Perú" },
        new Autor { Id = 4, Nombres = "Jorge", Apellidos = "Luis Borges", Nacionalidad = "Argentina" },
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
