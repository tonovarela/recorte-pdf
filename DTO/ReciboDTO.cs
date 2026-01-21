
using iText.Kernel.Geom;
namespace pdf_recorte.DTO;

public class ReciboDTO
{


    public required string BasePathDestino { get; set; }
    public required string CuentaProveedor { get; set; }

    public required string NumeroOperacion { get; set; }
    
    public required string NumeroProveedor { get; set; }

    public required string FechaOperacion { get; set; }

    public required int NumeroPagina  { get; set; }

    public required Rectangle AreaRecorte { get; set; }


   

    public string pathDestinoIndividual()
    {
        var (_, mes, anio) = FechaOperacion.Split("/").ToList() is var d ? (d[0], d[1], d[2]) : ("00", "00", "0000");
        string rutaDestinoIndividual = $"{BasePathDestino}/{NumeroProveedor}/{anio}/{mes}/PAGO_{NumeroOperacion}.pdf";
        return rutaDestinoIndividual;
    }

    public override string ToString()
    {
        return $"CuentaProveedor: {CuentaProveedor}, NumeroOperacion: {NumeroOperacion}, NumeroProveedor: {NumeroProveedor}, FechaOperacion: {FechaOperacion}, AreaRecorte: {AreaRecorte.ToString()}";
    }   



}
