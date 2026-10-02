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
    })
    .AddEntityFrameworkStores<BibliotecaContext>()
    .AddDefaultTokenProviders();

// Ruta de la pantalla de Login.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Cuenta/Login";
});

// AutorService usa Entity Framework Core (BibliotecaContext) para el CRUD de Autores.
builder.Services.AddScoped<IAutorService, AutorService>();

// Actividad: registro de ICategoriaService (ADO.NET) con ciclo de vida Scoped.
builder.Services.AddScoped<ICategoriaService, CategoriaService>();

// LibroService usa Entity Framework Core (BibliotecaContext) para el CRUD de Libros.
builder.Services.AddScoped<ILibroService, LibroService>();

var app = builder.Build();

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
