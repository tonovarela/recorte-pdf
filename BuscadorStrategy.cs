using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser.Data;
namespace pdf_recorte;

public class BuscadorStrategy : LocationTextExtractionStrategy
{
    private readonly string _inicio;
    private readonly string _fin;
    private  string _texto = string.Empty;
    
    public List<Rectangle> Inicios { get; } = new List<Rectangle>();
    public List<Rectangle> Fines { get; } = new List<Rectangle>();
    public List<string> CuentasProveedores { get; } = new List<string>();
    public List<string> NumerosOperacion { get; } = new List<string>(); 
    public List<string> NumerosProveedor { get; } = new List<string>();
    public List<string> FechasOperacion { get; } = new List<string>();
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
                     string cuenta_proveedor = ExtraerCuentaProveedor(this._texto);                                        
                    CuentasProveedores.Add(cuenta_proveedor);                                                        
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

 
private string ExtraerNumeroOperacion(string texto)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            texto,
            @"Número de operación\s*([0-9\s]+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (!match.Success) return string.Empty;

        var numero = new string(match.Groups[1].Value.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(numero) ? string.Empty : numero;
    }
    
       private string ExtraerCuentaProveedor(string texto)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            texto,
            @"Cuenta del proveedor\s*([0-9\s]+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (!match.Success) return string.Empty;

        var cuenta = new string(match.Groups[1].Value.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(cuenta) ? string.Empty : cuenta;
    }


     private string ExtraerNumeroProveedor(string texto)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            texto,
            @"(?:Clave\s+del\s+Proveedor|PROVEEDOR)\s*([0-9\s]+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!match.Success) return string.Empty;
        var numero = new string(match.Groups[1].Value.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(numero) ? string.Empty : numero;
    }


    private string ExtraerFechaOperacion(string texto)
    {
     
        var match = System.Text.RegularExpressions.Regex.Match(
            texto,
            @"Fecha\s+de\s+operación\s*([0-9]{2}/[0-9]{2}/[0-9]{4})",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!match.Success) return string.Empty;
        return match.Groups[1].Value.Trim();
    }
}
