using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BibliotecaMVC.Models;

namespace BibliotecaMVC.Controllers;

// Semana 11: Login, Registro y Cierre de sesión con ASP.NET Core Identity.
public class CuentaController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public CuentaController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // Actividad 2: Login
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Permite ingresar con el nombre de usuario o con el correo electrónico.
        var nombreUsuario = model.UsuarioOCorreo;
        if (model.UsuarioOCorreo.Contains("@"))
        {
            var usuario = await _userManager.FindByEmailAsync(model.UsuarioOCorreo);
            if (usuario != null)
            {
                nombreUsuario = usuario.UserName!;
            }
        }

        // Validación de credenciales con SignInManager.
        // lockoutOnFailure: true cuenta los intentos fallidos para bloquear la cuenta.
        var resultado = await _signInManager.PasswordSignInAsync(nombreUsuario, model.Password, model.Recordarme, true);

        if (resultado.Succeeded)
        {
            TempData["Mensaje"] = "Bienvenido a BibliotecaMVC.";
            return RedirectToAction("Index", "Home");
        }

        if (resultado.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Cuenta bloqueada por 5 intentos fallidos. Intente de nuevo en 15 minutos.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
        return View(model);
    }

    // Actividad 3: Registro
    public IActionResult Registro()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegistroViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = new IdentityUser
        {
            UserName = model.NombreUsuario,
            Email = model.Email
        };

        var resultado = await _userManager.CreateAsync(usuario, model.Password);

        if (resultado.Succeeded)
        {
            // Semana 12: todo usuario nuevo se registra con el rol Lector.
            await _userManager.AddToRoleAsync(usuario, "Lector");
            TempData["Mensaje"] = "Usuario registrado correctamente. Ya puede iniciar sesión.";
            return RedirectToAction(nameof(Login));
        }

        // Muestra los errores que devuelve Identity (contraseña débil, usuario repetido, etc.).
        foreach (var error in resultado.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    // Actividad 3: Cerrar sesión con SignInManager.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["Mensaje"] = "Sesión cerrada correctamente.";
        return RedirectToAction(nameof(Login));
    }

    // Semana 12: se muestra cuando el usuario no tiene el rol necesario.
    public IActionResult AccesoDenegado()
    {
        return View();
    }
}
