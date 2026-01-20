// See https://aka.ms/new-console-template for more information
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Kernel.Pdf.Canvas.Parser;
using pdf_recorte;

public partial class Program
{
    public static void Main(string[] args)
    {


       //LeerPDF("recortados/recorte_resultado_1.pdf");
        //return ;
        // --- CONFIGURACIÓN ---
        string rutaOrigen = "comprobante.pdf";
        string textoInicio = "Servicio Integral de Tesoreria (SIT)";
        string textoFin = "_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _";
        // --- PROCESO ---
        int contadorArchivos = 0;
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

                    // Márgenes
                    float margenIzq = 10f, margenAbajo = 10f, margenDer = 10f, margenArriba = 10f;
                    Rectangle areaRecorte = new Rectangle(
                        x - margenIzq,
                        y - margenAbajo,
                        width + margenIzq + margenDer,
                        height + margenArriba + margenAbajo
                    );

                    contadorArchivos++;
                    string rutaDestinoIndividual = $"recorte_resultado_{contadorArchivos}.pdf";

                    using (PdfWriter writer = new PdfWriter(rutaDestinoIndividual))
                    using (PdfDocument pdfDocDestino = new PdfDocument(writer))
                    {
                        // Crear nueva página SOLO del tamaño del bloque
                        PageSize pageSize = new PageSize(areaRecorte.GetWidth(), areaRecorte.GetHeight());
                        PdfPage nuevaPagina = pdfDocDestino.AddNewPage(pageSize);
                        PdfCanvas canvas = new PdfCanvas(nuevaPagina);

                        // Dibujar la página original como XObject, desplazando para que solo el bloque quede visible
                        PdfFormXObject xobj = paginaOrigen.CopyAsFormXObject(pdfDocDestino);
                        canvas.AddXObjectAt(xobj, -areaRecorte.GetX(), -areaRecorte.GetY());

                        Console.WriteLine($"Bloque {b + 1} en pág {i}. Creado '{rutaDestinoIndividual}'.");
                    }
                }
            }
        }
        Console.WriteLine("Proceso finalizado.");

    }


    public static void LeerPDF(string path){

       using (PdfReader reader = new PdfReader(path))
       using (PdfDocument pdfDoc = new PdfDocument(reader))
       {
           for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
           {
               PdfPage page = pdfDoc.GetPage(i);
               string text = PdfTextExtractor.GetTextFromPage(page);
               Console.WriteLine($"Texto de la página {i}: {text}");
           }
       }

    }
}
