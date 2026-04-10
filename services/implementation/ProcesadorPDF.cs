using iText.Kernel.Pdf;
using pdf_recorte.conf;
using pdf_recorte.DAO.interfaces;
using pdf_recorte.DTO;
using pdf_recorte.services.interfaces;
using pdf_recorte.utils;

namespace pdf_recorte.services.implementation;

public class ProcesadorPDF : IProcesadorPDF
{

    private readonly IAnexoDAO _anexoDAO;
    private readonly string _hotFolderPath;
    public ProcesadorPDF(IAnexoDAO anexoDAO, Conf conf )
    {
        _anexoDAO = anexoDAO;
        _hotFolderPath = conf.HotFolderPath;
    }

    public void Ejecutar()
    {        
        var archivosEntrada = ArchivoManager.Clasificar(_hotFolderPath);
        foreach (var origen in archivosEntrada.Where(a => a.Tipo == TipoArchivo.PLATAFORMA))
        {

            var recibos = ObtenerRecibosConAnexo(origen, _anexoDAO);        
            Console.WriteLine($"Procesando archivo: {origen.Ruta}");

            if (recibos.Count == 0)
            {
                Console.WriteLine("No se encontraron recibos con anexo asociado.");
                continue;
            }

            var rutas = recibos.Select(r => r.ArchivoAnexoDTO!.RutaArchivoSystem)
                               .ToList();

            bool directoriosCreados = PDFManager.CrearDirectoriosSiNoExisten(rutas);
            if (!directoriosCreados)
            {
                Console.WriteLine("No se pudieron crear todos los directorios necesarios. Verifique los errores anteriores.");
                continue;
            }

            using var reader = new PdfReader(origen.Ruta);
            using var pdfDocOrigen = new PdfDocument(reader);
            foreach (var recibo in recibos)
            {
                ProcesarRecibo(pdfDocOrigen, recibo, _anexoDAO);
            }
            //ArchivoManager.Eliminar(origen.Ruta);



        }
    }


     private static List<ReciboDTO> ObtenerRecibosConAnexo(ArchivoClasificado origen, IAnexoDAO anexoDAO)
    {
        return PDFManager.ObtenerRecibos(origen)
            .Select(r =>
            {
                r.ArchivoAnexoDTO = anexoDAO.obtener(r.Monto, r.NumeroProveedor, r.FechaOperacion);
                return r;
            })
            .Where(r => r.ArchivoAnexoDTO is not null)
            .ToList();
    }

    private static void ProcesarRecibo(PdfDocument pdfDocOrigen, ReciboDTO recibo, IAnexoDAO anexoDAO)
    {
        try
        {
            PDFManager.RecortarPagina(pdfDocOrigen, recibo);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Error de acceso al archivo: {ex.Message}");
            return;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error al recortar la página: {ex.Message}");
            return;
        }

        try
        {
            if (anexoDAO.existe(recibo.ArchivoAnexoDTO!))
                anexoDAO.borrar(recibo.ArchivoAnexoDTO!.Id.ToString());

            Console.WriteLine($"Archivo generado: {recibo.ArchivoAnexoDTO!.RutaArchivoSystem}");
            anexoDAO.registrar(recibo.ArchivoAnexoDTO!);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            Console.WriteLine($"Error al registrar el anexo en la base de datos: {ex.Message}");
            return;
        }
    }

}
