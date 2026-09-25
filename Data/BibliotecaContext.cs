using BibliotecaMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Data;

// Actividad 1: DbContext que Entity Framework Core usa para comunicarse
// con la base de datos SQL Server. Aquí se registra la entidad Libro
// mediante su DbSet correspondiente.
public class BibliotecaContext : DbContext
{
    public BibliotecaContext(DbContextOptions<BibliotecaContext> options)
        : base(options)
    {
    }

    public DbSet<Libro> Libros => Set<Libro>();
    public DbSet<Autor> Autores => Set<Autor>();
}
