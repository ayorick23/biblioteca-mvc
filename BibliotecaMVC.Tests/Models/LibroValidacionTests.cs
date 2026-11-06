using System.ComponentModel.DataAnnotations;
using BibliotecaMVC.Models;

namespace BibliotecaMVC.Tests.Models;

// Actividades 3 y 5: pruebas de las validaciones (Data Annotations) del modelo Libro.
public class LibroValidacionTests
{
    // Método auxiliar: valida el libro igual que lo hace MVC al llenar el ModelState.
    private static List<ValidationResult> Validar(Libro libro)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(libro, new ValidationContext(libro), resultados, true);
        return resultados;
    }

    [Fact]
    public void Libro_ConDatosValidos_NoTieneErrores()
    {
        var libro = new Libro { Titulo = "Cien años de soledad", Autor = "Gabriel García Márquez", CategoriaId = 1, Anio = 1967 };

        var errores = Validar(libro);

        Assert.Empty(errores);
    }

    [Fact]
    public void Libro_SinTitulo_TieneErrorDeTituloObligatorio()
    {
        var libro = new Libro { Titulo = "", Autor = "Autor de prueba", CategoriaId = 1 };

        var errores = Validar(libro);

        Assert.Single(errores);
        Assert.Equal("El título es obligatorio.", errores[0].ErrorMessage);
    }

    [Fact]
    public void Libro_SinAutor_TieneErrorDeAutorObligatorio()
    {
        var libro = new Libro { Titulo = "Libro de prueba", Autor = "", CategoriaId = 1 };

        var errores = Validar(libro);

        Assert.Single(errores);
        Assert.Equal("El autor es obligatorio.", errores[0].ErrorMessage);
    }

    [Fact]
    public void Libro_ConTituloDeMasDe150Caracteres_NoEsValido()
    {
        var libro = new Libro { Titulo = new string('A', 151), Autor = "Autor de prueba", CategoriaId = 1 };

        var errores = Validar(libro);

        Assert.False(errores.Count == 0, "Un título de 151 caracteres debería ser inválido.");
    }

    // [Theory]: el mismo caso se prueba con varios años fuera del rango 1000-2100.
    [Theory]
    [InlineData(999)]
    [InlineData(2101)]
    [InlineData(0)]
    public void Libro_ConAnioFueraDeRango_NoEsValido(int anio)
    {
        var libro = new Libro { Titulo = "Libro de prueba", Autor = "Autor de prueba", CategoriaId = 1, Anio = anio };

        var errores = Validar(libro);

        Assert.Single(errores);
        Assert.Equal("Ingrese un año válido.", errores[0].ErrorMessage);
    }

    // [Theory]: años en el límite del rango y año vacío (es opcional).
    [Theory]
    [InlineData(1000)]
    [InlineData(2024)]
    [InlineData(2100)]
    [InlineData(null)]
    public void Libro_ConAnioDentroDelRangoOVacio_EsValido(int? anio)
    {
        var libro = new Libro { Titulo = "Libro de prueba", Autor = "Autor de prueba", CategoriaId = 1, Anio = anio };

        var errores = Validar(libro);

        Assert.True(errores.Count == 0, $"El año {anio} debería ser válido.");
    }
}
