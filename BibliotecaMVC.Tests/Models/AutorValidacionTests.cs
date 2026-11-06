using System.ComponentModel.DataAnnotations;
using BibliotecaMVC.Models;

namespace BibliotecaMVC.Tests.Models;

// Pruebas de las validaciones (Data Annotations) del modelo Autor.
public class AutorValidacionTests
{
    // Método auxiliar: valida el autor igual que lo hace MVC al llenar el ModelState.
    private static List<ValidationResult> Validar(Autor autor)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(autor, new ValidationContext(autor), resultados, true);
        return resultados;
    }

    [Fact]
    public void Autor_ConDatosValidos_NoTieneErrores()
    {
        var autor = new Autor { Nombres = "Isabel", Apellidos = "Allende", Nacionalidad = "Chilena" };

        var errores = Validar(autor);

        Assert.Empty(errores);
    }

    // [Theory]: cada campo obligatorio vacío produce su propio mensaje de error.
    [Theory]
    [InlineData("", "Allende", "Chilena", "Los nombres son obligatorios.")]
    [InlineData("Isabel", "", "Chilena", "Los apellidos son obligatorios.")]
    [InlineData("Isabel", "Allende", "", "La nacionalidad es obligatoria.")]
    public void Autor_ConCampoObligatorioVacio_TieneErrorEsperado(string nombres, string apellidos, string nacionalidad, string mensajeEsperado)
    {
        var autor = new Autor { Nombres = nombres, Apellidos = apellidos, Nacionalidad = nacionalidad };

        var errores = Validar(autor);

        Assert.Single(errores);
        Assert.Equal(mensajeEsperado, errores[0].ErrorMessage);
    }

    [Fact]
    public void Autor_ConNombresDeMasDe100Caracteres_NoEsValido()
    {
        var autor = new Autor { Nombres = new string('A', 101), Apellidos = "Allende", Nacionalidad = "Chilena" };

        var errores = Validar(autor);

        Assert.False(errores.Count == 0, "Unos nombres de 101 caracteres deberían ser inválidos.");
    }
}
