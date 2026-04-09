using System;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Xobject;
using Microsoft.Extensions.DependencyModel.Resolution;
using pdf_recorte.conf;
using pdf_recorte.DTO;
using pdf_recorte.strategy;

namespace pdf_recorte.utils;

public class PDFManager
{


    public static List<ReciboDTO> ObtenerRecibos(ArchivoClasificado archivo)
    {
        var config = ConfigTipoArchivo.ObtenerPorTipo(archivo.Tipo);
        if (config is null)
            return [];

        var recibos = new List<ReciboDTO>();

        using var reader = new PdfReader(archivo.Ruta);
        using var pdfDocOrigen = new PdfDocument(reader);

        for (int i = 1; i <= pdfDocOrigen.GetNumberOfPages(); i++)
        {
            PdfPage paginaOrigen = pdfDocOrigen.GetPage(i);

            var estrategia = config.CrearEstrategia(config.TextoInicio, config.TextoFin);

            PdfTextExtractor.GetTextFromPage(paginaOrigen, estrategia);

            int bloques = Math.Min(estrategia.Inicios.Count, estrategia.Fines.Count);

            for (int b = 0; b < bloques; b++)
            {
                var areaRecorte = CalcularAreaRecorte(estrategia.Inicios[b], estrategia.Fines[b], config.MargenArriba);

                recibos.Add(new ReciboDTO
                {
                    NumeroPagina = i,
                    NumeroOperacion = estrategia.NumerosOperacion[b],
                    NumeroProveedor = estrategia.NumerosProveedor[b],
                    AreaRecorte = areaRecorte,
                    FechaOperacion = estrategia.FechasOperacion[b],
                    Monto = estrategia.Montos[b]
                });
            }
        }

        return recibos;
    }


    private static Rectangle CalcularAreaRecorte(Rectangle rectInicio, Rectangle rectFin, float margenArriba)
    {
        const float margenIzq = 10f, margenAbajo = 2f, margenDer = 10f;

        float x = Math.Min(rectInicio.GetX(), rectFin.GetX());
        float y = Math.Min(rectInicio.GetY(), rectFin.GetY());
        float maxX = Math.Max(rectInicio.GetX() + rectInicio.GetWidth(), rectFin.GetX() + rectFin.GetWidth());
        float maxY = Math.Max(rectInicio.GetY() + rectInicio.GetHeight(), rectFin.GetY() + rectFin.GetHeight());

        return new Rectangle(
            x - margenIzq,
            y - margenAbajo,
            (maxX - x) + margenIzq + margenDer,
            (maxY - y) + margenArriba + margenAbajo
        );
    }


    public static void RecortarPagina(PdfDocument pdfDocOrigen, ReciboDTO reciboDTO)
    {
        string destino = reciboDTO.ArchivoAnexoDTO!.RutaArchivoSystem;
        PdfPage paginaOrigen = pdfDocOrigen.GetPage(reciboDTO.NumeroPagina);
        Rectangle areaRecorte = reciboDTO.AreaRecorte;

        using var writer = new PdfWriter(destino);
        using var pdfDocDestino = new PdfDocument(writer);
                var pageSize = new PageSize(areaRecorte.GetWidth(), areaRecorte.GetHeight());
                PdfPage nuevaPagina = pdfDocDestino.AddNewPage(pageSize);

                var canvas = new PdfCanvas(nuevaPagina);
                PdfFormXObject xobj = paginaOrigen.CopyAsFormXObject(pdfDocDestino);
                canvas.AddXObjectAt(xobj, -areaRecorte.GetX(), -areaRecorte.GetY());
    }


    public static bool CrearDirectoriosSiNoExisten(List<string> rutasArchivos)
    {

       bool todosDirectoriosCreados = true;
        var directorios = rutasArchivos
           .Select(ruta => System.IO.Path.GetDirectoryName(ruta))
           .Where(dir => !string.IsNullOrEmpty(dir))
           .Distinct()
           .ToList();

        foreach (var dir in directorios)
        {
            try
            {
                Directory.CreateDirectory(dir!);                
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Error: No se tiene permiso para crear el directorio '{dir}'. Detalles: {ex.Message}");
                todosDirectoriosCreados = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear el directorio '{dir}': {ex.Message}");
                todosDirectoriosCreados = false;
            }
        }
        return todosDirectoriosCreados;

    }


}
