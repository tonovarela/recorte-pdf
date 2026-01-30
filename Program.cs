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
    static string _hotFolderPath = String.Empty;
    public static void Main(string[] args)
    {
        Conf conf = Conf.getInstance();
        _basePathDestino = conf.BasePathDestino;
        _hotFolderPath = conf.HotFolderPath;

        var archivosEntrada = obtenerArchivosEntrada();
        foreach (var origen in archivosEntrada)
        {
            List<ReciboDTO> recibos = ObtenerRecibos(origen);
            
            if (origen.Tipo == TipoArchivo.PLATAFORMA)
            {                                      
                //  crearDirectorioSiNoExiste(recibos);        
                // using (PdfReader reader = new PdfReader(origen.Ruta))
                // using (PdfDocument pdfDocOrigen = new PdfDocument(reader))
                //     foreach (var r in recibos)
                //         RecortarPagina(pdfDocOrigen, r);                                    
            }
            if  (origen.Tipo == TipoArchivo.CASH)
            {      

                foreach (var r in recibos)
                {
                    Console.WriteLine(r);
                }          
                // crearDirectorioSiNoExiste(recibos);        
                // using (PdfReader reader = new PdfReader(origen.Ruta))
                // using (PdfDocument pdfDocOrigen = new PdfDocument(reader))
                //     foreach (var r in recibos)
                //         RecortarPagina(pdfDocOrigen, r);                                    

            }
                           
        }
    }


    private static List<ArchivoClasificado> obtenerArchivosEntrada()
    {
        List<ArchivoClasificado> archivosClasificados = new List<ArchivoClasificado>();
        var archivos = Directory.GetFiles(_hotFolderPath, "*.pdf", SearchOption.AllDirectories);
        foreach (var archivo in archivos)
        {
            bool esCash = BuscarPalabraEnPdf(archivo, "BBVA Net Cash");
            if (esCash)
            {
                archivosClasificados.Add( new ArchivoClasificado {
                    Ruta = archivo,
                    Tipo = TipoArchivo.CASH
                });
                
                continue;
            }
            bool esPlataforma = BuscarPalabraEnPdf(archivo, "Servicio Integral de Tesoreria (SIT)");
            if (esPlataforma)
            {
                archivosClasificados.Add( new ArchivoClasificado { 
                    Ruta = archivo,
                    Tipo = TipoArchivo.PLATAFORMA
                });                
                continue;
            }            
        }
        return archivosClasificados;
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

    private static bool BuscarPalabraEnPdf(string rutaOrigen, string palabra)
    {
        List<int> paginasCoinciden = new List<int>();
        using (var reader = new PdfReader(rutaOrigen))
        using (var pdf = new PdfDocument(reader))
        {
            for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                var page = pdf.GetPage(i);
                var texto = PdfTextExtractor.GetTextFromPage(page);
                if (!string.IsNullOrEmpty(texto) &&
                    texto.IndexOf(palabra, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    paginasCoinciden.Add(i);
                }
            }
        }
        return paginasCoinciden.Count > 0;
    }

    private static List<ReciboDTO> ObtenerRecibos(ArchivoClasificado archivo)
    {
        List<ReciboDTO> recibos = new List<ReciboDTO>();

        string textoInicio = String.Empty;
        string textoFin = String.Empty;

        if (archivo.Tipo == TipoArchivo.PLATAFORMA){
                    textoInicio = "Servicio Integral de Tesoreria (SIT)";
                    textoFin = "_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _";
        }

        if (archivo.Tipo == TipoArchivo.CASH){
                    textoInicio = "BBVA Net Cash";
                    textoFin = "www.bbvanetcash.mx";
            
        }
                    

        using (PdfReader reader = new PdfReader(archivo.Ruta))
        using (PdfDocument pdfDocOrigen = new PdfDocument(reader))
        {
            for (int i = 1; i <= pdfDocOrigen.GetNumberOfPages(); i++)
            {
                PdfPage paginaOrigen = pdfDocOrigen.GetPage(i);
                SearchStrategy estrategia = archivo.Tipo == TipoArchivo.PLATAFORMA ?  new BuscadorStrategy(textoInicio, textoFin):
                                                                                      new SearchStrategy(textoInicio, textoFin);

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
                    float margenIzq = 10f, margenAbajo = 2f, margenDer = 10f, margenArriba = archivo.Tipo == TipoArchivo.CASH ? 30f : 20f;
                    Rectangle areaRecorte = new Rectangle(x - margenIzq, y - margenAbajo, width + margenIzq + margenDer, height + margenArriba + margenAbajo);
                    recibos.Add(new ReciboDTO
                    {
                        BasePathDestino = _basePathDestino,
                        NumeroPagina = i,                        
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
