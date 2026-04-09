using System;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using pdf_recorte.DTO;

namespace pdf_recorte.utils;

public class ArchivoManager
{

      public static List<ArchivoClasificado> Clasificar(string hotFolderPath)
    {
        var archivos = Directory.GetFiles(hotFolderPath, "*.pdf", SearchOption.AllDirectories);

        return archivos
            .Select(archivo =>
            {
                var tipo = DetectarTipoArchivo(archivo);
                return tipo.HasValue ? new ArchivoClasificado { Ruta = archivo, Tipo = tipo.Value } : null;
            })
            .Where(a => a is not null)
            .ToList()!;
    }

      private static TipoArchivo? DetectarTipoArchivo(string rutaPdf)
    {
        try
        {
            using var reader = new PdfReader(rutaPdf);
            using var pdf = new PdfDocument(reader);

            for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                var texto = PdfTextExtractor.GetTextFromPage(pdf.GetPage(i));
                if (string.IsNullOrEmpty(texto)) continue;


                if (texto.Contains("BBVA Net Cash", StringComparison.OrdinalIgnoreCase))
                    return TipoArchivo.CASH;

                if (texto.Contains("Servicio Integral de Tesoreria (SIT)", StringComparison.OrdinalIgnoreCase))
                    return TipoArchivo.PLATAFORMA;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error al leer {rutaPdf}: {ex.Message}");
        }

        return null;
    }

}
