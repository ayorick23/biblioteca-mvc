namespace BibliotecaMVC.Tests;

// Actividad 1: primera prueba sencilla para comprobar que el entorno de pruebas funciona.
public class EntornoPruebasTests
{
    [Fact]
    public void EntornoDePruebas_SumaSencilla_DevuelveResultadoEsperado()
    {
        // Arrange
        int a = 2;
        int b = 3;

        // Act
        int resultado = a + b;

        // Assert
        Assert.Equal(5, resultado);
    }
}
