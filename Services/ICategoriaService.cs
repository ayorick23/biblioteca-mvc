using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services;

public interface ICategoriaService
{
    List<Categoria> ObtenerCategorias();
    Categoria? ObtenerCategoriaPorId(int id);
    void CrearCategoria(Categoria categoria);
    void ActualizarCategoria(Categoria categoria);
    void EliminarCategoria(int id);
}
