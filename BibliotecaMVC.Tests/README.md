# Semana 16 – Pruebas unitarias (BibliotecaMVC)

## 1. Configuración del entorno

- Proyecto: `BibliotecaMVC.Tests` (xUnit, .NET 10), agregado a la solución `BibliotecaMVC.slnx`.
- Paquetes: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `coverlet.collector` y `Moq`.
- Referencia al proyecto `BibliotecaMVC.csproj` para acceder a controladores, servicios y modelos.
- El proyecto de pruebas está dentro de la carpeta de BibliotecaMVC, por eso en `BibliotecaMVC.csproj`
  se excluye la carpeta `BibliotecaMVC.Tests` (así el proyecto web no compila los archivos de prueba).
- Ejecución: Explorador de pruebas de Visual Studio o, desde la carpeta del proyecto, `dotnet test BibliotecaMVC.slnx`.

## 2. Organización

| Clase | Componente evaluado |
|---|---|
| `EntornoPruebasTests` | Prueba sencilla para comprobar que el entorno funciona |
| `Controllers/LibrosControllerTests` | `LibrosController` (consultar, registrar, editar, eliminar) |
| `Models/LibroValidacionTests` | Validaciones del modelo `Libro` |
| `Controllers/AutoresControllerTests` | `AutoresController` (consultar, registrar, editar, eliminar) |
| `Models/AutorValidacionTests` | Validaciones del modelo `Autor` |

**Límite de prueba elegido:** la lógica CRUD con Entity Framework está en `LibroService`/`AutorService`, que
dependen directamente de `BibliotecaContext` (SQL Server). Por eso se prueba el **controlador** y se simulan
sus dependencias mediante las interfaces `ILibroService`, `ICategoriaService` e `IAutorService` con Moq
(`Setup()`, `Returns()`, `Throws()`, `Verify()`). No se usa la base de datos real y no se modificó la
arquitectura del proyecto. Cada prueba crea sus propios mocks, así que pueden repetirse en cualquier orden.

## 3. Escenarios evaluados – Módulo Libros

| Operación | Escenario | Resultado esperado |
|---|---|---|
| Consulta (Index) | Existen libros | Vista con la lista (2 libros), se llama a `ObtenerLibros()` una vez |
| Consulta (Index) | No existen libros | Vista con lista vacía |
| Consulta (Index) | El servicio lanza una excepción | La excepción se relanza |
| Consulta (Detalle) | Libro existente | Vista con el libro correcto |
| Consulta (Detalle) | Libro inexistente (Id 0, 99, -1) – `[Theory]` | `NotFound` |
| Registro (Crear GET) | Abrir formulario | Vista y se cargan las categorías |
| Registro (Crear POST) | Datos válidos | Se llama a `CrearLibro()`, mensaje en TempData, redirige a Index |
| Registro (Crear POST) | Datos inválidos | Devuelve la vista, **no** se llama a `CrearLibro()` |
| Edición (Editar GET) | Libro existente / inexistente | Vista con el libro / `NotFound` |
| Edición (Editar POST) | Datos válidos | Se llama a `ActualizarLibro()` con los datos editados, redirige |
| Edición (Editar POST) | Datos inválidos | Devuelve la vista, **no** se llama a `ActualizarLibro()` |
| Eliminación | Libro existente | Se llama a `EliminarLibro(1)`, mensaje, redirige |
| Eliminación | Libro inexistente | Se llama al servicio y se muestra igualmente el mensaje de éxito (ver observaciones) |
| Validación del modelo | Datos válidos, título/autor vacíos, título > 150 caracteres | Sin errores / mensaje correspondiente |
| Validación del modelo | Año fuera de rango (999, 2101, 0) y dentro (1000, 2024, 2100, vacío) – `[Theory]` | Error "Ingrese un año válido." / sin errores |

El módulo **Autores** tiene los mismos escenarios (sin categorías) y sus validaciones de campos obligatorios
(nombres, apellidos, nacionalidad) se prueban con un `[Theory]`.

## 4. Resultados

```
Correctas! - Con error: 0, Superado: 49, Omitido: 0, Total: 49
```

(28 pruebas del módulo Libros + entorno, 21 del módulo Autores).

### Prueba que falla intencionalmente (Actividad 5)

En `Index_ConLibrosRegistrados_DevuelveVistaConLaLista` se cambió temporalmente `Assert.Equal(2, modelo.Count)`
por `Assert.Equal(3, modelo.Count)`:

```
[FAIL] LibrosControllerTests.Index_ConLibrosRegistrados_DevuelveVistaConLaLista
Assert.Equal() Failure: Values differ
Expected: 3
Actual:   2
Con error: 1, Superado: 27, Total: 28
```

**Análisis:** el mock devuelve 2 libros, así que el controlador entrega correctamente 2; el error estaba en
el valor esperado de la prueba, no en la aplicación. Se restauró el valor a 2 y la prueba volvió a pasar.

## 5. Observaciones / errores identificados

- **Eliminar un registro inexistente muestra mensaje de éxito.** `Eliminar(id)` no comprueba si el libro
  (o autor) existe; `EliminarLibro()` simplemente no hace nada y el usuario ve "El libro se eliminó
  correctamente." Lo mismo ocurre al editar un Id inexistente (`ActualizarLibro()` retorna sin avisar).
  Las pruebas documentan este comportamiento actual; no se cambió el código para no afectar la aplicación.
- **Casos aún no cubiertos:** `LibroService`/`AutorService` (consultas reales con EF Core, orden por título,
  nombre de categoría) no se prueban aquí porque dependen del `DbContext`; corresponden a pruebas de
  integración. Tampoco se prueban los permisos por rol (`[Authorize]`), que los aplica el pipeline de ASP.NET.
- El proyecto principal muestra una advertencia de NuGet por `System.Security.Cryptography.Pkcs`, que viene de
  `Microsoft.Data.SqlClient`; no está relacionada con las pruebas.

## 6. Tipos de prueba (actividad complementaria)

| Nivel | Qué verifica en el módulo Libros | Ejemplo |
|---|---|---|
| **Unitaria** | Un método aislado, con dependencias simuladas | `LibrosController.Crear()` con un mock de `ILibroService` (lo implementado aquí) |
| **Integración** | Que varias piezas reales funcionen juntas | Controlador + `LibroService` + base de datos de prueba (EF Core InMemory/SQLite o `WebApplicationFactory`) para comprobar que un libro creado realmente se guarda y se lista |
| **Extremo a extremo (E2E)** | Un flujo completo como lo hace el usuario | Con un navegador automatizado (p. ej. Playwright/Selenium): iniciar sesión, abrir el catálogo, registrar un libro y verlo en el listado |

Las unitarias son rápidas y aíslan errores de lógica; las de integración detectan problemas entre capas
(consultas, configuración); las E2E validan la experiencia completa, pero son más lentas y frágiles.
