using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Kernel.Pdf.Canvas.Parser;

using pdf_recorte.DTO;
using Path = System.IO.Path;
using pdf_recorte.conf;
using pdf_recorte.strategy;

public partial class Program
{
    static string _basePathDestino = String.Empty;
    public static void Main(string[] args)
    {
        Conf conf = Conf.getInstance();
        _basePathDestino = conf.BasePathDestino;
        var origen = "comprobante.pdf";
        List<ReciboDTO> recibos = ObtenerRecibos(origen);
        crearDirectorioSiNoExiste(recibos);        
        using (PdfReader reader = new PdfReader(origen))
        using (PdfDocument pdfDocOrigen = new PdfDocument(reader))
            foreach (var r in recibos)
                RecortarPagina(pdfDocOrigen, r);                            
    }


    private static void crearDirectorioSiNoExiste(List<ReciboDTO> recibos)
    {
         recibos
        .Select(x => Path.GetDirectoryName(x.pathDestinoIndividual()))
        .Distinct()
        .ToList()
        .ForEach(dir =>
        {
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        });
    }

    private static List<ReciboDTO> ObtenerRecibos(string rutaOrigen)
    {
        List<ReciboDTO> recibos = new List<ReciboDTO>();
        string textoInicio = "Servicio Integral de Tesoreria (SIT)";
        string textoFin = "_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _";
        using (PdfReader reader = new PdfReader(rutaOrigen))
        using (PdfDocument pdfDocOrigen = new PdfDocument(reader))
        {
            for (int i = 1; i <= pdfDocOrigen.GetNumberOfPages(); i++)
            {
                PdfPage paginaOrigen = pdfDocOrigen.GetPage(i);
                var estrategia = new BuscadorStrategy(textoInicio, textoFin);
                PdfTextExtractor.GetTextFromPage(paginaOrigen, estrategia);
                int bloques = Math.Min(estrategia.Inicios.Count, estrategia.Fines.Count);
                for (int b = 0; b < bloques; b++)
                {
                    var rectInicio = estrategia.Inicios[b];
                    var rectFin = estrategia.Fines[b];
                    float x = Math.Min(rectInicio.GetX(), rectFin.GetX());
                    float y = Math.Min(rectInicio.GetY(), rectFin.GetY());
                    float maxX = Math.Max(rectInicio.GetX() + rectInicio.GetWidth(), rectFin.GetX() + rectFin.GetWidth());
                    float maxY = Math.Max(rectInicio.GetY() + rectInicio.GetHeight(), rectFin.GetY() + rectFin.GetHeight());
                    float width = maxX - x;
                    float height = maxY - y;
                    float margenIzq = 10f, margenAbajo = 2f, margenDer = 10f, margenArriba = 20f;
                    Rectangle areaRecorte = new Rectangle(x - margenIzq, y - margenAbajo, width + margenIzq + margenDer, height + margenArriba + margenAbajo);
                    recibos.Add(new ReciboDTO
                    {
                        BasePathDestino = _basePathDestino,
                        NumeroPagina = i,
                        CuentaProveedor = estrategia.CuentasProveedores[b],
                        NumeroOperacion = estrategia.NumerosOperacion[b],
                        NumeroProveedor = estrategia.NumerosProveedor[b],
                        AreaRecorte = areaRecorte,
                        FechaOperacion = estrategia.FechasOperacion[b],
                    });
                }
            }
        }
        return recibos;

    }


     private static void RecortarPagina(PdfDocument pdfDocOrigen, ReciboDTO reciboDTO)
    {
        string destino = reciboDTO.pathDestinoIndividual();        
        PdfPage paginaOrigen = pdfDocOrigen.GetPage(reciboDTO.NumeroPagina);
        using (PdfWriter writer = new PdfWriter(destino))
        using (PdfDocument pdfDocDestino = new PdfDocument(writer))
        {
            Rectangle areaRecorte = reciboDTO.AreaRecorte;
            PageSize pageSize = new PageSize(areaRecorte.GetWidth(), areaRecorte.GetHeight());
            PdfPage nuevaPagina = pdfDocDestino.AddNewPage(pageSize);
            PdfCanvas canvas = new PdfCanvas(nuevaPagina);
            PdfFormXObject xobj = paginaOrigen.CopyAsFormXObject(pdfDocDestino);
            canvas.AddXObjectAt(xobj, -areaRecorte.GetX(), -areaRecorte.GetY());
        }
    }
}
