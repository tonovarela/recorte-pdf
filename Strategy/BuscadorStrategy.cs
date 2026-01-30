
namespace pdf_recorte.strategy;

public class BuscadorStrategy :SearchStrategy
{
    
    public BuscadorStrategy(string textoInicio, string textoFin): base(textoInicio, textoFin)
    {
                
    }

    
protected  override string ExtraerNumeroOperacion(string texto)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            texto,
            @"Número de operación\s*([0-9\s]+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (!match.Success) return string.Empty;

        var numero = new string(match.Groups[1].Value.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(numero) ? string.Empty : numero;
    }
    



     protected  override  string ExtraerNumeroProveedor(string texto)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            texto,
            @"(?:Clave\s+del\s+Proveedor|PROVEEDOR)\s*([0-9\s]+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!match.Success) return string.Empty;
        var numero = new string(match.Groups[1].Value.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(numero) ? string.Empty : numero;
    }


    protected override string ExtraerFechaOperacion(string texto)
    {
     
        var match = System.Text.RegularExpressions.Regex.Match(
            texto,
            @"Fecha\s+de\s+operación\s*([0-9]{2}/[0-9]{2}/[0-9]{4})",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!match.Success) return string.Empty;
        return match.Groups[1].Value.Trim();
    }
}
