using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models;

// Datos del formulario de inicio de sesión.
public class LoginViewModel
{
    [Required(ErrorMessage = "Ingrese su usuario o correo electrónico.")]
    [Display(Name = "Usuario o correo electrónico")]
    public string UsuarioOCorreo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese su contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Recordarme")]
    public bool Recordarme { get; set; }
}
