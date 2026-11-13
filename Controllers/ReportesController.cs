using BibliotecaMVC.Data;
using BibliotecaMVC.Models;
using BibliotecaMVC.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BibliotecaMVC.Controllers;

// Semana 17: reportes del módulo Libros en PDF (QuestPDF) y Excel (ClosedXML).
[Authorize(Roles = "Admin,Bibliotecario,Usuario")]
public class ReportesController : Controller
{
    private readonly BibliotecaContext _context;
    private readonly ICategoriaService _categoriaService;

    public ReportesController(BibliotecaContext context, ICategoriaService categoriaService)
    {
        _context = context;
        _categoriaService = categoriaService;
    }

    // Consulta los libros con Entity Framework Core aplicando los filtros (Actividad 6).
    // El autor es un campo del libro y la categoría se obtiene con CategoriaId.
    private List<Libro> ObtenerLibros(string? autor, int? categoriaId)
    {
        var consulta = _context.Libros.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(autor))
        {
            consulta = consulta.Where(l => l.Autor == autor);
        }

        if (categoriaId.HasValue)
        {
            consulta = consulta.Where(l => l.CategoriaId == categoriaId.Value);
        }

        var libros = consulta.OrderBy(l => l.Id).ToList();

        // Se completa el nombre de la categoría de cada libro.
        var categorias = _categoriaService.ObtenerCategorias();
        foreach (var libro in libros)
        {
            libro.CategoriaNombre = categorias.FirstOrDefault(c => c.Id == libro.CategoriaId)?.Nombre ?? "-";
        }

        return libros;
    }

    // Texto con los filtros aplicados, para mostrarlo en los reportes.
    private string DescribirFiltros(string? autor, int? categoriaId)
    {
        var categoria = categoriaId.HasValue ? _categoriaService.ObtenerCategoriaPorId(categoriaId.Value)?.Nombre : null;
        return $"Autor: {(string.IsNullOrEmpty(autor) ? "Todos" : autor)} | Categoría: {categoria ?? "Todas"}";
    }

    // Vista de reportería con los filtros y la vista previa de los libros.
    public IActionResult Index(string? autor, int? categoriaId)
    {
        ViewBag.Autores = _context.Libros.Select(l => l.Autor).Distinct().OrderBy(a => a).ToList();
        ViewBag.Categorias = _categoriaService.ObtenerCategorias();
        ViewBag.Autor = autor;
        ViewBag.CategoriaId = categoriaId;

        return View(ObtenerLibros(autor, categoriaId));
    }

    // Actividades 2 y 3: genera y descarga el reporte en PDF con QuestPDF.
    public IActionResult Pdf(string? autor, int? categoriaId)
    {
        var libros = ObtenerLibros(autor, categoriaId);
        var filtros = DescribirFiltros(autor, categoriaId);
        var colorPrincipal = Colors.Blue.Darken3;

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Encabezado: nombre del sistema, título, fecha y filtros.
                page.Header().Column(col =>
                {
                    col.Item().Text("BibliotecaMVC").FontSize(20).Bold().FontColor(colorPrincipal);
                    col.Item().Text("Reporte de Libros").FontSize(14).SemiBold();
                    col.Item().Text($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}").FontColor(Colors.Grey.Darken1);
                    col.Item().Text($"Filtros: {filtros}").FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(5).LineHorizontal(2).LineColor(colorPrincipal);
                });

                // Contenido: tabla de libros y resumen.
                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(40);  // ID
                            c.RelativeColumn(3);   // Nombre
                            c.RelativeColumn(2);   // Autor
                            c.RelativeColumn(2);   // Categoría
                            c.ConstantColumn(50);  // Año
                        });

                        tabla.Header(h =>
                        {
                            foreach (var titulo in new[] { "ID", "Nombre del libro", "Autor", "Categoría", "Año" })
                            {
                                h.Cell().Background(colorPrincipal).Element(Celda).AlignCenter()
                                    .Text(titulo).FontColor(Colors.White).Bold();
                            }
                        });

                        for (int i = 0; i < libros.Count; i++)
                        {
                            var libro = libros[i];
                            // Filas intercaladas para facilitar la lectura.
                            var fondo = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                            tabla.Cell().Background(fondo).Element(Celda).AlignCenter().Text(libro.Id.ToString());
                            tabla.Cell().Background(fondo).Element(Celda).Text(libro.Titulo);
                            tabla.Cell().Background(fondo).Element(Celda).Text(libro.Autor);
                            tabla.Cell().Background(fondo).Element(Celda).Text(libro.CategoriaNombre);
                            tabla.Cell().Background(fondo).Element(Celda).AlignCenter().Text(libro.Anio?.ToString() ?? "-");
                        }
                    });

                    if (libros.Count == 0)
                    {
                        col.Item().PaddingTop(10).Text("No hay libros que coincidan con los filtros.").Italic();
                    }

                    col.Item().PaddingTop(10).AlignRight().Text($"Total de libros: {libros.Count}").Bold().FontColor(colorPrincipal);
                });

                // Pie de página con la numeración.
                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Página ");
                    t.CurrentPageNumber();
                    t.Span(" de ");
                    t.TotalPages();
                });
            });
        });

        return File(documento.GeneratePdf(), "application/pdf", "ReporteLibros.pdf");
    }

    // Estilo común de las celdas de la tabla del PDF.
    private static IContainer Celda(IContainer contenedor) =>
        contenedor.Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5);

    // Actividad 5: genera y descarga el reporte en Excel con ClosedXML.
    public IActionResult Excel(string? autor, int? categoriaId)
    {
        var libros = ObtenerLibros(autor, categoriaId);
        var colorPrincipal = XLColor.FromHtml("#1F4E79");

        using var libroExcel = new XLWorkbook();
        var hoja = libroExcel.Worksheets.Add("Reporte de Libros");

        // Título, fecha de generación y filtros.
        hoja.Cell(1, 1).Value = "BibliotecaMVC - Reporte de Libros";
        var titulo = hoja.Range(1, 1, 1, 5).Merge();
        titulo.Style.Font.Bold = true;
        titulo.Style.Font.FontSize = 16;
        titulo.Style.Font.FontColor = XLColor.White;
        titulo.Style.Fill.BackgroundColor = colorPrincipal;
        titulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        hoja.Cell(2, 1).Value = $"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}";
        hoja.Range(2, 1, 2, 5).Merge();
        hoja.Cell(3, 1).Value = $"Filtros: {DescribirFiltros(autor, categoriaId)}";
        hoja.Range(3, 1, 3, 5).Merge();

        // Encabezados de la tabla (fila 5).
        var encabezados = new[] { "ID", "Nombre del libro", "Autor", "Categoría", "Año" };
        for (int i = 0; i < encabezados.Length; i++)
        {
            hoja.Cell(5, i + 1).Value = encabezados[i];
        }

        var rangoEncabezados = hoja.Range(5, 1, 5, 5);
        rangoEncabezados.Style.Font.Bold = true;
        rangoEncabezados.Style.Font.FontColor = XLColor.White;
        rangoEncabezados.Style.Fill.BackgroundColor = colorPrincipal;
        rangoEncabezados.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Registros de libros.
        int fila = 6;
        foreach (var libro in libros)
        {
            hoja.Cell(fila, 1).Value = libro.Id;
            hoja.Cell(fila, 2).Value = libro.Titulo;
            hoja.Cell(fila, 3).Value = libro.Autor;
            hoja.Cell(fila, 4).Value = libro.CategoriaNombre;
            hoja.Cell(fila, 5).Value = libro.Anio?.ToString() ?? "-";

            // Filas intercaladas.
            if (fila % 2 == 1)
            {
                hoja.Range(fila, 1, fila, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
            }
            fila++;
        }

        // Bordes de toda la tabla y alineación de ID y Año.
        var tabla = hoja.Range(5, 1, fila - 1, 5);
        tabla.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        tabla.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        hoja.Range(6, 1, fila, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        hoja.Range(6, 5, fila, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Resumen con el total de libros.
        hoja.Cell(fila + 1, 4).Value = "Total de libros:";
        hoja.Cell(fila + 1, 5).Value = libros.Count;
        hoja.Range(fila + 1, 4, fila + 1, 5).Style.Font.Bold = true;

        // Ancho automático de columnas (desde la fila de encabezados).
        hoja.Columns(1, 5).AdjustToContents(5, fila + 1);

        using var stream = new MemoryStream();
        libroExcel.SaveAs(stream);

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "ReporteLibros.xlsx");
    }

    // ===================== Reportes de Autores =====================

    // Consulta los autores con Entity Framework Core, con filtro opcional por nacionalidad.
    private List<Autor> ObtenerAutores(string? nacionalidad)
    {
        var consulta = _context.Autores.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(nacionalidad))
        {
            consulta = consulta.Where(a => a.Nacionalidad == nacionalidad);
        }

        return consulta.OrderBy(a => a.Id).ToList();
    }

    // Vista de reportería de autores con el filtro y la vista previa.
    public IActionResult Autores(string? nacionalidad)
    {
        ViewBag.Nacionalidades = _context.Autores.Select(a => a.Nacionalidad).Distinct().OrderBy(n => n).ToList();
        ViewBag.Nacionalidad = nacionalidad;

        return View(ObtenerAutores(nacionalidad));
    }

    // Genera y descarga el reporte de autores en PDF con QuestPDF.
    public IActionResult AutoresPdf(string? nacionalidad)
    {
        var autores = ObtenerAutores(nacionalidad);
        var filtros = $"Nacionalidad: {(string.IsNullOrEmpty(nacionalidad) ? "Todas" : nacionalidad)}";
        var colorPrincipal = Colors.Teal.Darken3;

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("BibliotecaMVC").FontSize(20).Bold().FontColor(colorPrincipal);
                    col.Item().Text("Reporte de Autores").FontSize(14).SemiBold();
                    col.Item().Text($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}").FontColor(Colors.Grey.Darken1);
                    col.Item().Text($"Filtros: {filtros}").FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(5).LineHorizontal(2).LineColor(colorPrincipal);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(40);  // ID
                            c.RelativeColumn(2);   // Nombres
                            c.RelativeColumn(2);   // Apellidos
                            c.RelativeColumn(2);   // Nacionalidad
                        });

                        tabla.Header(h =>
                        {
                            foreach (var titulo in new[] { "ID", "Nombres", "Apellidos", "Nacionalidad" })
                            {
                                h.Cell().Background(colorPrincipal).Element(Celda).AlignCenter()
                                    .Text(titulo).FontColor(Colors.White).Bold();
                            }
                        });

                        for (int i = 0; i < autores.Count; i++)
                        {
                            var autor = autores[i];
                            var fondo = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                            tabla.Cell().Background(fondo).Element(Celda).AlignCenter().Text(autor.Id.ToString());
                            tabla.Cell().Background(fondo).Element(Celda).Text(autor.Nombres);
                            tabla.Cell().Background(fondo).Element(Celda).Text(autor.Apellidos);
                            tabla.Cell().Background(fondo).Element(Celda).Text(autor.Nacionalidad);
                        }
                    });

                    if (autores.Count == 0)
                    {
                        col.Item().PaddingTop(10).Text("No hay autores que coincidan con los filtros.").Italic();
                    }

                    col.Item().PaddingTop(10).AlignRight().Text($"Total de autores: {autores.Count}").Bold().FontColor(colorPrincipal);
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Página ");
                    t.CurrentPageNumber();
                    t.Span(" de ");
                    t.TotalPages();
                });
            });
        });

        return File(documento.GeneratePdf(), "application/pdf", "ReporteAutores.pdf");
    }

    // Genera y descarga el reporte de autores en Excel con ClosedXML.
    public IActionResult AutoresExcel(string? nacionalidad)
    {
        var autores = ObtenerAutores(nacionalidad);
        var colorPrincipal = XLColor.FromHtml("#00695C");

        using var libroExcel = new XLWorkbook();
        var hoja = libroExcel.Worksheets.Add("Reporte de Autores");

        // Título, fecha de generación y filtros.
        hoja.Cell(1, 1).Value = "BibliotecaMVC - Reporte de Autores";
        var titulo = hoja.Range(1, 1, 1, 4).Merge();
        titulo.Style.Font.Bold = true;
        titulo.Style.Font.FontSize = 16;
        titulo.Style.Font.FontColor = XLColor.White;
        titulo.Style.Fill.BackgroundColor = colorPrincipal;
        titulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        hoja.Cell(2, 1).Value = $"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}";
        hoja.Range(2, 1, 2, 4).Merge();
        hoja.Cell(3, 1).Value = $"Filtros: Nacionalidad: {(string.IsNullOrEmpty(nacionalidad) ? "Todas" : nacionalidad)}";
        hoja.Range(3, 1, 3, 4).Merge();

        // Encabezados de la tabla (fila 5).
        var encabezados = new[] { "ID", "Nombres", "Apellidos", "Nacionalidad" };
        for (int i = 0; i < encabezados.Length; i++)
        {
            hoja.Cell(5, i + 1).Value = encabezados[i];
        }

        var rangoEncabezados = hoja.Range(5, 1, 5, 4);
        rangoEncabezados.Style.Font.Bold = true;
        rangoEncabezados.Style.Font.FontColor = XLColor.White;
        rangoEncabezados.Style.Fill.BackgroundColor = colorPrincipal;
        rangoEncabezados.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Registros de autores.
        int fila = 6;
        foreach (var autor in autores)
        {
            hoja.Cell(fila, 1).Value = autor.Id;
            hoja.Cell(fila, 2).Value = autor.Nombres;
            hoja.Cell(fila, 3).Value = autor.Apellidos;
            hoja.Cell(fila, 4).Value = autor.Nacionalidad;

            if (fila % 2 == 1)
            {
                hoja.Range(fila, 1, fila, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
            }
            fila++;
        }

        // Bordes de la tabla y alineación del ID.
        var tabla = hoja.Range(5, 1, fila - 1, 4);
        tabla.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        tabla.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        hoja.Range(6, 1, fila, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Resumen con el total de autores.
        hoja.Cell(fila + 1, 3).Value = "Total de autores:";
        hoja.Cell(fila + 1, 4).Value = autores.Count;
        hoja.Range(fila + 1, 3, fila + 1, 4).Style.Font.Bold = true;

        hoja.Columns(1, 4).AdjustToContents(5, fila + 1);

        using var stream = new MemoryStream();
        libroExcel.SaveAs(stream);

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "ReporteAutores.xlsx");
    }
}
