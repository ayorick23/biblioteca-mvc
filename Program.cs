using BibliotecaMVC.Data;
using BibliotecaMVC.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Actividad 1: registro del DbContext de Entity Framework Core, usando la
// misma cadena de conexión a SQL Server que ya usan los demás módulos.
var connectionString = builder.Configuration.GetConnectionString("BibliotecaMVC")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'BibliotecaMVC'.");
builder.Services.AddDbContext<BibliotecaContext>(options => options.UseSqlServer(connectionString));

// Actividad 3: registro de IAutorService con ciclo de vida Scoped.
// Actividad 5: para usar la segunda implementación, basta con cambiar
// esta línea a AddScoped<IAutorService, AutorServiceAlterno>() sin tocar el controlador.
builder.Services.AddScoped<IAutorService, AutorService>();

// Actividad: registro de ICategoriaService (ADO.NET) con ciclo de vida Scoped.
builder.Services.AddScoped<ICategoriaService, CategoriaService>();

// Actividades 2 y 3: LibroService ahora usa Entity Framework Core (BibliotecaContext).
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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
