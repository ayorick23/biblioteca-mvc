using BibliotecaMVC.Data;
using BibliotecaMVC.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Actividad 1: registro del DbContext de Entity Framework Core, usando la
// misma cadena de conexión a SQL Server que ya usan los demás módulos.
var connectionString = builder.Configuration.GetConnectionString("BibliotecaMVC")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'BibliotecaMVC'.");
builder.Services.AddDbContext<BibliotecaContext>(options => options.UseSqlServer(connectionString));

// Semana 11 - Actividad 1: configuración de ASP.NET Core Identity con IdentityUser,
// guardando los usuarios mediante Entity Framework Core en BibliotecaContext.
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;

        // Bloqueo de la cuenta por 15 minutos tras 5 intentos fallidos de login.
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<BibliotecaContext>()
    .AddErrorDescriber<ErroresIdentityEspanol>()
    .AddDefaultTokenProviders();

// Ruta de la pantalla de Login.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Cuenta/Login";
    // Semana 12: página que se muestra cuando el usuario no tiene el rol necesario.
    options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
});

// AutorService usa Entity Framework Core (BibliotecaContext) para el CRUD de Autores.
builder.Services.AddScoped<IAutorService, AutorService>();

// Actividad: registro de ICategoriaService (ADO.NET) con ciclo de vida Scoped.
builder.Services.AddScoped<ICategoriaService, CategoriaService>();

// LibroService usa Entity Framework Core (BibliotecaContext) para el CRUD de Libros.
builder.Services.AddScoped<ILibroService, LibroService>();

var app = builder.Build();

// Semana 13: los roles (Admin, Bibliotecario y Usuario) y sus usuarios también se pueden
// crear en SQL Server con el script Scripts/RolesYUsuarios.sql.
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Actividad 1: se crea cada rol solo si todavía no existe, para evitar duplicados.
    foreach (var rol in new[] { "Admin", "Bibliotecario", "Usuario" })
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            var resultado = await roleManager.CreateAsync(new IdentityRole(rol));
            if (!resultado.Succeeded)
            {
                app.Logger.LogError("No se pudo crear el rol {Rol}.", rol);
            }
        }
    }

    // A los usuarios que no tienen un rol asignado se les asigna el rol Usuario.
    var usuarios = userManager.Users.ToList();

    foreach (var usuario in usuarios)
    {
        var roles = await userManager.GetRolesAsync(usuario);
        if (roles.Count == 0)
        {
            var resultado = await userManager.AddToRoleAsync(usuario, "Usuario");
            if (!resultado.Succeeded)
            {
                app.Logger.LogError("No se pudo asignar el rol Usuario a {Usuario}.", usuario.UserName);
            }
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
