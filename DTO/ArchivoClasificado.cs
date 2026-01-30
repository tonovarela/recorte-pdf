

namespace pdf_recorte.DTO;


public enum TipoArchivo {
        CASH,
        PLATAFORMA
    }

public class ArchivoClasificado
{
       public string Ruta { get; set; } =String.Empty;
        public TipoArchivo Tipo { get; set; }

}

