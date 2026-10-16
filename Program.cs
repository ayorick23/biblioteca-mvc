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

// Semana 12 - Actividad 2: creación de roles y asignación a los usuarios.
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Roles de la aplicación: Administrador (gestiona libros) y Lector (solo consulta).
    foreach (var rol in new[] { "Administrador", "Lector" })
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }

    // Usuario administrador por defecto.
    var admin = await userManager.FindByNameAsync("admin");
    if (admin == null)
    {
        admin = new IdentityUser { UserName = "admin", Email = "admin@biblioteca.com" };
        await userManager.CreateAsync(admin, "Admin123!");
    }
    if (!await userManager.IsInRoleAsync(admin, "Administrador"))
    {
        await userManager.AddToRoleAsync(admin, "Administrador");
    }

    // Los usuarios existentes que aún no tienen rol quedan como Lector.
    foreach (var usuario in userManager.Users.ToList())
    {
        if ((await userManager.GetRolesAsync(usuario)).Count == 0)
        {
            await userManager.AddToRoleAsync(usuario, "Lector");
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
