
using pdf_recorte.DTO;
using pdf_recorte.conf;
using pdf_recorte.DAO;
using pdf_recorte.utils;
using iText.Kernel.Pdf;
using Microsoft.Extensions.DependencyModel.Resolution;


public partial class Program
{
    private static readonly Conf _conf = Conf.getInstance();
    private static string HotFolderPath => _conf.HotFolderPath;


    public static void Main(string[] args)
    {
        AnexoDAO anexoDAO = new AnexoDAO();
        var archivosEntrada =  ArchivoManager.Clasificar(HotFolderPath);
        foreach (var origen in archivosEntrada.Where(a => a.Tipo == TipoArchivo.PLATAFORMA))
        {                        
            var recibos = PDFManager.ObtenerRecibos(origen)
                .Select(r =>
                {
                    r.ArchivoAnexoDTO = anexoDAO.obtener(r.Monto, r.NumeroProveedor, r.FechaOperacion);
                    return r;
                })
                .Where(r => r.ArchivoAnexoDTO is not null)
                .ToList();

            if (recibos.Count == 0)
            {
                Console.WriteLine("No se encontraron recibos con anexo asociado.");
                continue;
            }

            var rutas = recibos.Where(r=>r.ArchivoAnexoDTO is not null)
                               .Select(r=>r.ArchivoAnexoDTO!.RutaArchivoSystem)
                               .ToList();

           bool directoriosCreados = PDFManager.CrearDirectoriosSiNoExisten(rutas);
           if (!directoriosCreados)
            {
                Console.WriteLine("No se pudieron crear todos los directorios necesarios. Verifique los errores anteriores.");
                continue;
            }

                using var reader = new PdfReader(origen.Ruta);
                using var pdfDocOrigen = new PdfDocument(reader);
                foreach (var r in recibos)
                {
                    
                   try
                    {
                    PDFManager.RecortarPagina(pdfDocOrigen, r);
                    if (anexoDAO.existe(r.ArchivoAnexoDTO!))
                         anexoDAO.borrar(r.ArchivoAnexoDTO!.Id.ToString());
                         
                    Console.WriteLine($"Archivo generado: {r.ArchivoAnexoDTO!.RutaArchivoSystem}");
                    anexoDAO.registrar(r.ArchivoAnexoDTO!);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Console.WriteLine($"Error de acceso al archivo: {ex.Message}");
                        continue; // Continuar con el siguiente recibo
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al recortar la página: {ex.Message}");
                        continue; // Continuar con el siguiente recibo
                    }
                    
                    
                }


             }            
        }
    


}
