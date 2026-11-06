using BibliotecaMVC.Controllers;
using BibliotecaMVC.Models;
using BibliotecaMVC.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;

namespace BibliotecaMVC.Tests.Controllers;

// Actividades 2, 3 y 4: pruebas unitarias del módulo Libros.
// Se prueba LibrosController de forma aislada: ILibroService e ICategoriaService
// se simulan con Moq, así no se usa la base de datos real.
public class LibrosControllerTests
{
    private readonly Mock<ILibroService> _libroServiceMock;
    private readonly Mock<ICategoriaService> _categoriaServiceMock;
    private readonly LibrosController _controller;

    // xUnit crea una instancia nueva de esta clase por cada prueba, así cada
    // prueba tiene sus propios mocks y no depende del orden de ejecución.
    public LibrosControllerTests()
    {
        _libroServiceMock = new Mock<ILibroService>();
        _categoriaServiceMock = new Mock<ICategoriaService>();
        var loggerMock = new Mock<ILogger<LibrosController>>();

        _categoriaServiceMock.Setup(s => s.ObtenerCategorias()).Returns(new List<Categoria>());

        _controller = new LibrosController(_libroServiceMock.Object, _categoriaServiceMock.Object, loggerMock.Object);

        // TempData es necesario porque Crear, Editar y Eliminar guardan un mensaje en él.
        _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
    }

    private static List<Libro> LibrosDePrueba()
    {
        return new List<Libro>
        {
            new Libro { Id = 1, Titulo = "Cien años de soledad", Autor = "Gabriel García Márquez", CategoriaId = 1, Anio = 1967 },
            new Libro { Id = 2, Titulo = "Don Quijote de la Mancha", Autor = "Miguel de Cervantes", CategoriaId = 1, Anio = 1605 }
        };
    }

    // ---------- Consulta: Index ----------

    [Fact]
    public void Index_ConLibrosRegistrados_DevuelveVistaConLaLista()
    {
        // Arrange
        _libroServiceMock.Setup(s => s.ObtenerLibros()).Returns(LibrosDePrueba());

        // Act
        var resultado = _controller.Index();

        // Assert
        var vista = Assert.IsType<ViewResult>(resultado);
        var modelo = Assert.IsType<List<Libro>>(vista.Model);
        Assert.NotNull(modelo);
        Assert.Equal(2, modelo.Count);
        Assert.Equal("Cien años de soledad", modelo[0].Titulo);
        _libroServiceMock.Verify(s => s.ObtenerLibros(), Times.Once);
    }

    [Fact]
    public void Index_SinLibrosRegistrados_DevuelveVistaConListaVacia()
    {
        _libroServiceMock.Setup(s => s.ObtenerLibros()).Returns(new List<Libro>());

        var resultado = _controller.Index();

        var vista = Assert.IsType<ViewResult>(resultado);
        var modelo = Assert.IsType<List<Libro>>(vista.Model);
        Assert.Empty(modelo);
    }

    [Fact]
    public void Index_CuandoElServicioFalla_RelanzaLaExcepcion()
    {
        _libroServiceMock.Setup(s => s.ObtenerLibros()).Throws(new Exception("Error de base de datos"));

        Assert.Throws<Exception>(() => _controller.Index());
    }

    // ---------- Consulta: Detalle ----------

    [Fact]
    public void Detalle_ConLibroExistente_DevuelveVistaConElLibro()
    {
        _libroServiceMock.Setup(s => s.ObtenerLibroPorId(1)).Returns(LibrosDePrueba()[0]);

        var resultado = _controller.Detalle(1);

        var vista = Assert.IsType<ViewResult>(resultado);
        var modelo = Assert.IsType<Libro>(vista.Model);
        Assert.NotNull(modelo);
        Assert.Equal(1, modelo.Id);
        Assert.Equal("Gabriel García Márquez", modelo.Autor);
    }

    // [Theory]: varios Id que no existen (el mock devuelve null).
    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    [InlineData(-1)]
    public void Detalle_ConLibroInexistente_DevuelveNotFound(int id)
    {
        _libroServiceMock.Setup(s => s.ObtenerLibroPorId(id)).Returns((Libro?)null);

        var resultado = _controller.Detalle(id);

        Assert.IsType<NotFoundResult>(resultado);
        _libroServiceMock.Verify(s => s.ObtenerLibroPorId(id), Times.Once);
    }

    // ---------- Registro: Crear ----------

    [Fact]
    public void Crear_Get_DevuelveVistaYCargaCategorias()
    {
        var resultado = _controller.Crear();

        Assert.IsType<ViewResult>(resultado);
        Assert.NotNull(_controller.ViewBag.Categorias);
        _categoriaServiceMock.Verify(s => s.ObtenerCategorias(), Times.Once);
    }

    [Fact]
    public void Crear_ConDatosValidos_GuardaElLibroYRedirigeAIndex()
    {
        var libro = new Libro { Titulo = "Rayuela", Autor = "Julio Cortázar", CategoriaId = 1, Anio = 1963 };

        var resultado = _controller.Crear(libro);

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redireccion.ActionName);
        Assert.Equal("El libro se agregó correctamente.", _controller.TempData["Mensaje"]);
        _libroServiceMock.Verify(s => s.CrearLibro(libro), Times.Once);
    }

    [Fact]
    public void Crear_ConDatosInvalidos_DevuelveLaVistaYNoGuarda()
    {
        var libro = new Libro { Titulo = "", Autor = "Julio Cortázar", CategoriaId = 1 };
        // En una prueba unitaria MVC no valida el modelo, por eso el error se agrega manualmente.
        _controller.ModelState.AddModelError("Titulo", "El título es obligatorio.");

        var resultado = _controller.Crear(libro);

        var vista = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(libro, vista.Model);
        Assert.False(_controller.ModelState.IsValid);
        Assert.NotNull(_controller.ViewBag.Categorias);
        _libroServiceMock.Verify(s => s.CrearLibro(It.IsAny<Libro>()), Times.Never);
    }

    // ---------- Edición: Editar ----------

    [Fact]
    public void Editar_Get_ConLibroExistente_DevuelveVistaConElLibro()
    {
        _libroServiceMock.Setup(s => s.ObtenerLibroPorId(2)).Returns(LibrosDePrueba()[1]);

        var resultado = _controller.Editar(2);

        var vista = Assert.IsType<ViewResult>(resultado);
        var modelo = Assert.IsType<Libro>(vista.Model);
        Assert.Equal("Don Quijote de la Mancha", modelo.Titulo);
        Assert.NotNull(_controller.ViewBag.Categorias);
    }

    [Fact]
    public void Editar_Get_ConLibroInexistente_DevuelveNotFound()
    {
        _libroServiceMock.Setup(s => s.ObtenerLibroPorId(50)).Returns((Libro?)null);

        var resultado = _controller.Editar(50);

        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public void Editar_Post_ConDatosValidos_ActualizaYRedirigeAIndex()
    {
        var libro = new Libro { Id = 1, Titulo = "Cien años de soledad (edición revisada)", Autor = "Gabriel García Márquez", CategoriaId = 2, Anio = 1967 };

        var resultado = _controller.Editar(libro);

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redireccion.ActionName);
        Assert.Equal("El libro se actualizó correctamente.", _controller.TempData["Mensaje"]);
        // Se verifica que al servicio le llegó el libro con los datos editados.
        _libroServiceMock.Verify(s => s.ActualizarLibro(It.Is<Libro>(l =>
            l.Id == 1 && l.Titulo == "Cien años de soledad (edición revisada)" && l.CategoriaId == 2)), Times.Once);
    }

    [Fact]
    public void Editar_Post_ConDatosInvalidos_DevuelveLaVistaYNoActualiza()
    {
        var libro = new Libro { Id = 1, Titulo = "Libro", Autor = "Autor", CategoriaId = 1, Anio = 3000 };
        _controller.ModelState.AddModelError("Anio", "Ingrese un año válido.");

        var resultado = _controller.Editar(libro);

        var vista = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(libro, vista.Model);
        _libroServiceMock.Verify(s => s.ActualizarLibro(It.IsAny<Libro>()), Times.Never);
    }

    // ---------- Eliminación: Eliminar ----------

    [Fact]
    public void Eliminar_ConLibroExistente_LlamaAlServicioYRedirigeAIndex()
    {
        var resultado = _controller.Eliminar(1);

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redireccion.ActionName);
        Assert.Equal("El libro se eliminó correctamente.", _controller.TempData["Mensaje"]);
        _libroServiceMock.Verify(s => s.EliminarLibro(1), Times.Once);
    }

    [Fact]
    public void Eliminar_ConLibroInexistente_IgualMuestraMensajeDeExito()
    {
        // El controlador no comprueba si el libro existe antes de eliminarlo:
        // el servicio no hace nada y aun así se informa "eliminado".
        // Esta prueba documenta ese comportamiento (ver README, observaciones).
        var resultado = _controller.Eliminar(99);

        Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("El libro se eliminó correctamente.", _controller.TempData["Mensaje"]);
        _libroServiceMock.Verify(s => s.EliminarLibro(99), Times.Once);
        _libroServiceMock.Verify(s => s.ObtenerLibroPorId(It.IsAny<int>()), Times.Never);
    }
}
