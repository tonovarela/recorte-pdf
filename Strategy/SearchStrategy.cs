using System;

namespace pdf_recorte.strategy;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser.Data;

public class SearchStrategy:LocationTextExtractionStrategy
{

    protected  string _inicio =String.Empty;
    protected  string _fin = String.Empty;
    protected  string _texto = string.Empty;
    
    public List<Rectangle> Inicios { get; } = new List<Rectangle>();
    public List<Rectangle> Fines { get; } = new List<Rectangle>();
    
    public List<string> NumerosOperacion { get; } = new List<string>(); 
    public List<string> NumerosProveedor { get; } = new List<string>();
    public List<string> FechasOperacion { get; } = new List<string>();


 public SearchStrategy(string textoInicio, string textoFin)
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
                 this._texto+= texto;
                if (texto.Contains(_inicio, StringComparison.OrdinalIgnoreCase))
                {                    
                    var rect = renderInfo.GetBaseline().GetBoundingRectangle();
                    Inicios.Add(rect);
                }
                else if (texto.Contains(_fin, StringComparison.OrdinalIgnoreCase))
                {
                    var rect = renderInfo.GetBaseline().GetBoundingRectangle();
                    Fines.Add(rect);                                                                                                
                    string numero_operacion = ExtraerNumeroOperacion(this._texto);
                    NumerosOperacion.Add(numero_operacion);
                    string numero_proveedor = ExtraerNumeroProveedor(this._texto);
                    NumerosProveedor.Add(numero_proveedor);
                    string fecha_operacion = ExtraerFechaOperacion(this._texto);
                    FechasOperacion.Add(fecha_operacion);
                    this._texto= string.Empty;
                }                                        
            }                        
        }
        
        base.EventOccurred(data, type);
    }

    protected virtual string ExtraerFechaOperacion(string texto)
    {    
         var match = System.Text.RegularExpressions.Regex.Match(
            texto,
            @"Fecha de aplicación:\s*([0-9]{2}/[0-9]{2}/[0-9]{4})",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!match.Success) return string.Empty;
        return match.Groups[1].Value.Trim();
    }
    protected virtual string ExtraerNumeroProveedor(string texto)
    {
        return "TEST";
    }
    
    protected virtual string ExtraerNumeroOperacion(string texto)
    {
        //Folio de firma: 
        var match = System.Text.RegularExpressions.Regex.Match(
            texto,
            @"Folio de firma:\s*([0-9\s]+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (!match.Success) return string.Empty;

        var numero = new string(match.Groups[1].Value.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(numero) ? string.Empty : numero;
        
    }

}
