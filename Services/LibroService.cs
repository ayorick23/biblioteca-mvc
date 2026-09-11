using BibliotecaMVC.Models;
using Microsoft.Data.SqlClient;

namespace BibliotecaMVC.Services;

public class LibroService : ILibroService
{
    private readonly string _connectionString;

    public LibroService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("BibliotecaMVC")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'BibliotecaMVC'.");
    }

    private const string SelectBase =
        "SELECT l.Id, l.Titulo, l.Autor, l.CategoriaId, c.Nombre, l.Anio " +
        "FROM Libros l INNER JOIN Categorias c ON l.CategoriaId = c.Id";

    private static Libro LeerLibro(SqlDataReader lector)
    {
        return new Libro
        {
            Id = lector.GetInt32(0),
            Titulo = lector.GetString(1),
            Autor = lector.GetString(2),
            CategoriaId = lector.GetInt32(3),
            CategoriaNombre = lector.GetString(4),
            Anio = lector.IsDBNull(5) ? null : lector.GetInt32(5)
        };
    }

    public List<Libro> ObtenerLibros()
    {
        var libros = new List<Libro>();

        using var conexion = new SqlConnection(_connectionString);
        using var comando = new SqlCommand(SelectBase + " ORDER BY l.Titulo", conexion);

        conexion.Open();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            libros.Add(LeerLibro(lector));
        }

        return libros;
    }

    public Libro? ObtenerLibroPorId(int id)
    {
        using var conexion = new SqlConnection(_connectionString);
        using var comando = new SqlCommand(SelectBase + " WHERE l.Id = @Id", conexion);
        comando.Parameters.AddWithValue("@Id", id);

        conexion.Open();
        using var lector = comando.ExecuteReader();
        if (lector.Read())
        {
            return LeerLibro(lector);
        }

        return null;
    }

    public void CrearLibro(Libro libro)
    {
        using var conexion = new SqlConnection(_connectionString);
        using var comando = new SqlCommand(
            "INSERT INTO Libros (Titulo, Autor, CategoriaId, Anio) VALUES (@Titulo, @Autor, @CategoriaId, @Anio)", conexion);
        comando.Parameters.AddWithValue("@Titulo", libro.Titulo);
        comando.Parameters.AddWithValue("@Autor", libro.Autor);
        comando.Parameters.AddWithValue("@CategoriaId", libro.CategoriaId);
        comando.Parameters.AddWithValue("@Anio", (object?)libro.Anio ?? DBNull.Value);

        conexion.Open();
        comando.ExecuteNonQuery();
    }

    public void ActualizarLibro(Libro libro)
    {
        using var conexion = new SqlConnection(_connectionString);
        using var comando = new SqlCommand(
            "UPDATE Libros SET Titulo = @Titulo, Autor = @Autor, CategoriaId = @CategoriaId, Anio = @Anio WHERE Id = @Id", conexion);
        comando.Parameters.AddWithValue("@Titulo", libro.Titulo);
        comando.Parameters.AddWithValue("@Autor", libro.Autor);
        comando.Parameters.AddWithValue("@CategoriaId", libro.CategoriaId);
        comando.Parameters.AddWithValue("@Anio", (object?)libro.Anio ?? DBNull.Value);
        comando.Parameters.AddWithValue("@Id", libro.Id);

        conexion.Open();
        comando.ExecuteNonQuery();
    }

    public void EliminarLibro(int id)
    {
        using var conexion = new SqlConnection(_connectionString);
        using var comando = new SqlCommand("DELETE FROM Libros WHERE Id = @Id", conexion);
        comando.Parameters.AddWithValue("@Id", id);

        conexion.Open();
        comando.ExecuteNonQuery();
    }
}
