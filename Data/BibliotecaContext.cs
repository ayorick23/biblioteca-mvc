using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Data;

// Actividad 1: DbContext que Entity Framework Core usa para comunicarse
// con la base de datos SQL Server. Aquí se registra la entidad Libro
// mediante su DbSet correspondiente.
// Semana 11: ahora hereda de IdentityDbContext<IdentityUser> para que
// Identity guarde los usuarios en la misma base de datos (tablas AspNet*).
public class BibliotecaContext : IdentityDbContext<IdentityUser>
{
    public BibliotecaContext(DbContextOptions<BibliotecaContext> options)
        : base(options)
    {
    }

    public DbSet<Libro> Libros => Set<Libro>();
    public DbSet<Autor> Autores => Set<Autor>();
}
