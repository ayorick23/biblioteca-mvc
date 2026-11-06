using BibliotecaMVC.Controllers;
using BibliotecaMVC.Models;
using BibliotecaMVC.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;

namespace BibliotecaMVC.Tests.Controllers;

// Pruebas unitarias del módulo Autores (mismo enfoque que el módulo Libros).
// IAutorService se simula con Moq, así no se usa la base de datos real.
public class AutoresControllerTests
{
    private readonly Mock<IAutorService> _autorServiceMock;
    private readonly AutoresController _controller;

    public AutoresControllerTests()
    {
        _autorServiceMock = new Mock<IAutorService>();
        var loggerMock = new Mock<ILogger<AutoresController>>();

        _controller = new AutoresController(_autorServiceMock.Object, loggerMock.Object);

        // TempData es necesario porque Crear, Editar y Eliminar guardan un mensaje en él.
        _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
    }

    private static List<Autor> AutoresDePrueba()
    {
        return new List<Autor>
        {
            new Autor { Id = 1, Nombres = "Gabriel", Apellidos = "García Márquez", Nacionalidad = "Colombiana" },
            new Autor { Id = 2, Nombres = "Julio", Apellidos = "Cortázar", Nacionalidad = "Argentina" }
        };
    }

    // ---------- Consulta: Index ----------

    [Fact]
    public void Index_ConAutoresRegistrados_DevuelveVistaConLaLista()
    {
        // Arrange
        _autorServiceMock.Setup(s => s.ObtenerAutores()).Returns(AutoresDePrueba());

        // Act
        var resultado = _controller.Index();

        // Assert
        var vista = Assert.IsType<ViewResult>(resultado);
        var modelo = Assert.IsType<List<Autor>>(vista.Model);
        Assert.NotNull(modelo);
        Assert.Equal(2, modelo.Count);
        Assert.Equal("Gabriel", modelo[0].Nombres);
        _autorServiceMock.Verify(s => s.ObtenerAutores(), Times.Once);
    }

    [Fact]
    public void Index_SinAutoresRegistrados_DevuelveVistaConListaVacia()
    {
        _autorServiceMock.Setup(s => s.ObtenerAutores()).Returns(new List<Autor>());

        var resultado = _controller.Index();

        var vista = Assert.IsType<ViewResult>(resultado);
        var modelo = Assert.IsType<List<Autor>>(vista.Model);
        Assert.Empty(modelo);
    }

    [Fact]
    public void Index_CuandoElServicioFalla_RelanzaLaExcepcion()
    {
        _autorServiceMock.Setup(s => s.ObtenerAutores()).Throws(new Exception("Error de base de datos"));

        Assert.Throws<Exception>(() => _controller.Index());
    }

    // ---------- Consulta: Detalle ----------

    [Fact]
    public void Detalle_ConAutorExistente_DevuelveVistaConElAutor()
    {
        _autorServiceMock.Setup(s => s.ObtenerAutorPorId(2)).Returns(AutoresDePrueba()[1]);

        var resultado = _controller.Detalle(2);

        var vista = Assert.IsType<ViewResult>(resultado);
        var modelo = Assert.IsType<Autor>(vista.Model);
        Assert.NotNull(modelo);
        Assert.Equal(2, modelo.Id);
        Assert.Equal("Cortázar", modelo.Apellidos);
    }

    // [Theory]: varios Id que no existen (el mock devuelve null).
    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    [InlineData(-1)]
    public void Detalle_ConAutorInexistente_DevuelveNotFound(int id)
    {
        _autorServiceMock.Setup(s => s.ObtenerAutorPorId(id)).Returns((Autor?)null);

        var resultado = _controller.Detalle(id);

        Assert.IsType<NotFoundResult>(resultado);
        _autorServiceMock.Verify(s => s.ObtenerAutorPorId(id), Times.Once);
    }

    // ---------- Registro: Crear ----------

    [Fact]
    public void Crear_Get_DevuelveVista()
    {
        var resultado = _controller.Crear();

        Assert.IsType<ViewResult>(resultado);
    }

    [Fact]
    public void Crear_ConDatosValidos_GuardaElAutorYRedirigeAIndex()
    {
        var autor = new Autor { Nombres = "Isabel", Apellidos = "Allende", Nacionalidad = "Chilena" };

        var resultado = _controller.Crear(autor);

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redireccion.ActionName);
        Assert.Equal("El autor se agregó correctamente.", _controller.TempData["Mensaje"]);
        _autorServiceMock.Verify(s => s.CrearAutor(autor), Times.Once);
    }

    [Fact]
    public void Crear_ConDatosInvalidos_DevuelveLaVistaYNoGuarda()
    {
        var autor = new Autor { Nombres = "", Apellidos = "Allende", Nacionalidad = "Chilena" };
        // En una prueba unitaria MVC no valida el modelo, por eso el error se agrega manualmente.
        _controller.ModelState.AddModelError("Nombres", "Los nombres son obligatorios.");

        var resultado = _controller.Crear(autor);

        var vista = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(autor, vista.Model);
        Assert.False(_controller.ModelState.IsValid);
        _autorServiceMock.Verify(s => s.CrearAutor(It.IsAny<Autor>()), Times.Never);
    }

    // ---------- Edición: Editar ----------

    [Fact]
    public void Editar_Get_ConAutorExistente_DevuelveVistaConElAutor()
    {
        _autorServiceMock.Setup(s => s.ObtenerAutorPorId(1)).Returns(AutoresDePrueba()[0]);

        var resultado = _controller.Editar(1);

        var vista = Assert.IsType<ViewResult>(resultado);
        var modelo = Assert.IsType<Autor>(vista.Model);
        Assert.Equal("García Márquez", modelo.Apellidos);
    }

    [Fact]
    public void Editar_Get_ConAutorInexistente_DevuelveNotFound()
    {
        _autorServiceMock.Setup(s => s.ObtenerAutorPorId(50)).Returns((Autor?)null);

        var resultado = _controller.Editar(50);

        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public void Editar_Post_ConDatosValidos_ActualizaYRedirigeAIndex()
    {
        var autor = new Autor { Id = 2, Nombres = "Julio Florencio", Apellidos = "Cortázar", Nacionalidad = "Argentina" };

        var resultado = _controller.Editar(autor);

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redireccion.ActionName);
        Assert.Equal("El autor se actualizó correctamente.", _controller.TempData["Mensaje"]);
        // Se verifica que al servicio le llegó el autor con los datos editados.
        _autorServiceMock.Verify(s => s.ActualizarAutor(It.Is<Autor>(a =>
            a.Id == 2 && a.Nombres == "Julio Florencio")), Times.Once);
    }

    [Fact]
    public void Editar_Post_ConDatosInvalidos_DevuelveLaVistaYNoActualiza()
    {
        var autor = new Autor { Id = 2, Nombres = "Julio", Apellidos = "Cortázar", Nacionalidad = "" };
        _controller.ModelState.AddModelError("Nacionalidad", "La nacionalidad es obligatoria.");

        var resultado = _controller.Editar(autor);

        var vista = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(autor, vista.Model);
        _autorServiceMock.Verify(s => s.ActualizarAutor(It.IsAny<Autor>()), Times.Never);
    }

    // ---------- Eliminación: Eliminar ----------

    [Fact]
    public void Eliminar_ConAutorExistente_LlamaAlServicioYRedirigeAIndex()
    {
        var resultado = _controller.Eliminar(1);

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redireccion.ActionName);
        Assert.Equal("El autor se eliminó correctamente.", _controller.TempData["Mensaje"]);
        _autorServiceMock.Verify(s => s.EliminarAutor(1), Times.Once);
    }

    [Fact]
    public void Eliminar_ConAutorInexistente_IgualMuestraMensajeDeExito()
    {
        // Igual que en Libros: no se comprueba si el autor existe antes de eliminarlo.
        var resultado = _controller.Eliminar(99);

        Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("El autor se eliminó correctamente.", _controller.TempData["Mensaje"]);
        _autorServiceMock.Verify(s => s.EliminarAutor(99), Times.Once);
        _autorServiceMock.Verify(s => s.ObtenerAutorPorId(It.IsAny<int>()), Times.Never);
    }
}
