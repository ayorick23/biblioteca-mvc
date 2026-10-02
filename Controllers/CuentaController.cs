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
        var resultado = await _signInManager.PasswordSignInAsync(nombreUsuario, model.Password, model.Recordarme, false);

        if (resultado.Succeeded)
        {
            TempData["Mensaje"] = "Bienvenido a BibliotecaMVC.";
            return RedirectToAction("Index", "Home");
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
}
