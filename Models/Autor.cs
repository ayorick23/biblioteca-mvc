using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models;

// Entidad Autor usada por Entity Framework Core (tabla Autores).
public class Autor
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los nombres no pueden superar los 100 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los apellidos no pueden superar los 100 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nacionalidad es obligatoria.")]
    [StringLength(100, ErrorMessage = "La nacionalidad no puede superar los 100 caracteres.")]
    public string Nacionalidad { get; set; } = string.Empty;
}
