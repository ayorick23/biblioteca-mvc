using BibliotecaMVC.Models;
using Microsoft.Data.SqlClient;

namespace BibliotecaMVC.Services;

public class CategoriaService : ICategoriaService
{
    private readonly string _connectionString;

    public CategoriaService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("BibliotecaMVC")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'BibliotecaMVC'.");
    }

    public List<Categoria> ObtenerCategorias()
    {
        var categorias = new List<Categoria>();

        using var conexion = new SqlConnection(_connectionString);
        using var comando = new SqlCommand("SELECT Id, Nombre, Descripcion FROM Categorias ORDER BY Nombre", conexion);

        conexion.Open();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            categorias.Add(new Categoria
            {
                Id = lector.GetInt32(0),
                Nombre = lector.GetString(1),
                Descripcion = lector.IsDBNull(2) ? string.Empty : lector.GetString(2)
            });
        }

        return categorias;
    }

    public Categoria? ObtenerCategoriaPorId(int id)
    {
        using var conexion = new SqlConnection(_connectionString);
        using var comando = new SqlCommand("SELECT Id, Nombre, Descripcion FROM Categorias WHERE Id = @Id", conexion);
        comando.Parameters.AddWithValue("@Id", id);

        conexion.Open();
        using var lector = comando.ExecuteReader();
        if (lector.Read())
        {
            return new Categoria
            {
                Id = lector.GetInt32(0),
                Nombre = lector.GetString(1),
                Descripcion = lector.IsDBNull(2) ? string.Empty : lector.GetString(2)
            };
        }

        return null;
    }

    public void CrearCategoria(Categoria categoria)
    {
        using var conexion = new SqlConnection(_connectionString);
        using var comando = new SqlCommand("INSERT INTO Categorias (Nombre, Descripcion) VALUES (@Nombre, @Descripcion)", conexion);
        comando.Parameters.AddWithValue("@Nombre", categoria.Nombre);
        comando.Parameters.AddWithValue("@Descripcion", categoria.Descripcion);

        conexion.Open();
        comando.ExecuteNonQuery();
    }

    public void ActualizarCategoria(Categoria categoria)
    {
        using var conexion = new SqlConnection(_connectionString);
        using var comando = new SqlCommand("UPDATE Categorias SET Nombre = @Nombre, Descripcion = @Descripcion WHERE Id = @Id", conexion);
        comando.Parameters.AddWithValue("@Nombre", categoria.Nombre);
        comando.Parameters.AddWithValue("@Descripcion", categoria.Descripcion);
        comando.Parameters.AddWithValue("@Id", categoria.Id);

        conexion.Open();
        comando.ExecuteNonQuery();
    }

    public void EliminarCategoria(int id)
    {
        using var conexion = new SqlConnection(_connectionString);
        using var comando = new SqlCommand("DELETE FROM Categorias WHERE Id = @Id", conexion);
        comando.Parameters.AddWithValue("@Id", id);

        conexion.Open();
        comando.ExecuteNonQuery();
    }
}
