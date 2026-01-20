using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser.Data;
namespace pdf_recorte;

public class BuscadorStrategy : LocationTextExtractionStrategy
{
    private readonly string _inicio;
    private readonly string _fin;

    public List<Rectangle> Inicios { get; } = new List<Rectangle>();
    public List<Rectangle> Fines { get; } = new List<Rectangle>();

    public BuscadorStrategy(string textoInicio, string textoFin)
    {
        _inicio = textoInicio;
        _fin = textoFin;
    }

    public override void EventOccurred(IEventData data, EventType type)
    {
        if (type == EventType.RENDER_TEXT)
        {
            TextRenderInfo renderInfo = (TextRenderInfo)data;
            var texto = renderInfo.GetText();
            if (!string.IsNullOrEmpty(texto))
            {
                if (texto.Contains(_inicio, StringComparison.OrdinalIgnoreCase))
                {
                    var rect = renderInfo.GetBaseline().GetBoundingRectangle();
                    Inicios.Add(rect);
                }
                else if (texto.Contains(_fin, StringComparison.OrdinalIgnoreCase))
                {
                    var rect = renderInfo.GetBaseline().GetBoundingRectangle();
                    Fines.Add(rect);
                }
            }
        }
        base.EventOccurred(data, type);
    }
}
