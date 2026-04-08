

namespace pdf_recorte.DTO;

public class ArchivoAnexoDTO
{
        public int Id { get; set; }
        private string _rutaArchivo = string.Empty;
        public required string RutaArchivoSystem { get
                {
                        if (!OperatingSystem.IsWindows()) // Previo tiene que estar montado el recurso en MAC  o Linux 
                                return _rutaArchivo.Replace('\\', '/').Replace("192.168.2.217","Volumes");        
                        return _rutaArchivo;
                } set => _rutaArchivo = value; }        
        
        public required string RutaArchivoDB { get; set; }
        
        public required string NombreArchivo { get; set; }
        public required string Tipo { get; set; }

}
