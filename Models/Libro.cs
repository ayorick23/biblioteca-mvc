using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BibliotecaMVC.Models;

// Actividad 1: entidad Libro preparada para Entity Framework Core.
// Las anotaciones de validación se usan en el formulario de Agregar (Actividad 5).
public class Libro
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(150, ErrorMessage = "El título no puede superar los 150 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El autor es obligatorio.")]
    [StringLength(100, ErrorMessage = "El autor no puede superar los 100 caracteres.")]
    public string Autor { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe seleccionar una categoría.")]
    [Display(Name = "Categoría")]
    public int CategoriaId { get; set; }

    // Propiedad de solo lectura para mostrar el nombre de la categoría en las
    // vistas. No existe como columna en la tabla Libros, por lo que EF Core
    // no debe intentar mapearla.
    [NotMapped]
    public string CategoriaNombre { get; set; } = string.Empty;

    [Range(1000, 2100, ErrorMessage = "Ingrese un año válido.")]
    [Display(Name = "Año")]
    public int? Anio { get; set; }
}
