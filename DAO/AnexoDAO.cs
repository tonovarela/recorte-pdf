using System;

namespace pdf_recorte.DAO;

using Microsoft.Data.SqlClient;
using pdf_recorte.DTO;

public class AnexoDAO : DAO
{


    public ArchivoAnexoDTO? obtener(string monto, string contacto, string fechaEmision)
    {
       ArchivoAnexoDTO? anexo = null;
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = $@"Select 
                                       'DIN', 
                                        ID,
                                        IDR='?', 
                                        Nombre='CP-Cheque Electronico-'+ MovID+'.pdf',
                                        Direccion=Ltrim(Rtrim('\\192.168.2.217\intelisis\CFD\ContabilidadElectronica\XML_PROV\LITO\COMPROBANTES_PAGO\'
                                                    +Ltrim(Rtrim(convert(char, year(FechaEmision))))+'\'+Ltrim(Rtrim(Convert(char,Month(FechaEmision))))+'\'+
                                                    +Ltrim(Rtrim(Convert(Char,Contacto)))+'\'+'CP-Cheque Electronico-'+ MovID+'.pdf')),
                                        Icono=66, 
                                        Tipo='Archivo', 
                                        Orden=1,
                                        Sucursal=0, 
                                        FechaEmision,
                                        TipoDocumento='CP',
                                        Alta=FechaEmision, 
                                        UltimoCambio=FechaEmision,
                                        Usuario='SCORREA'
                                        --FechaEmision, Importe, Moneda, Contacto  DATOS QUE VIENEN EN EL COMPROBANTE
                                        From Lito.dbo.Dinero
                                        Where  1=1
                                        and Mov='Cheque Electronico' 
                                        and Estatus in ('Concluido', 'Conciliado') 
                                        and Moneda in ('Pesos', 'Dolares')
                                        and FechaEmision= @fechaEmision
                                        and Importe= @monto
                                        and Contacto = @contacto"
                                        ;

                using (SqlCommand command = new SqlCommand(query, connection))
                {                    
                    command.Parameters.AddWithValue("@fechaEmision", fechaEmision);
                    command.Parameters.AddWithValue("@monto", monto);
                    command.Parameters.AddWithValue("@contacto", contacto);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string? id = reader["ID"].ToString();
                            string? nombre = reader["Nombre"].ToString();
                            string? direccion = reader["Direccion"].ToString();
                            string? tipo = reader["TipoDocumento"].ToString();                            
                            if (id != null && nombre != null && direccion != null && tipo != null)
                            {                            
                                anexo = new ArchivoAnexoDTO
                                {
                                    Id = int.Parse(id),
                                    NombreArchivo = nombre,
                                    RutaArchivoSystem = direccion,
                                    RutaArchivoDB = direccion,
                                    Tipo = tipo!
                                };
                            }                            
                            if (reader.Read())
                            {
                                Console.WriteLine("Se encontró más de una fila, se devuelve null");
                                anexo = null;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener datos: {ex.Message}");
        }
        return anexo;

    }



    public void  borrar(string id){
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"Delete From Lito_jess.dbo.anexomov where ID=@id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);                    
                    command.ExecuteNonQuery();                    
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al borrar anexo: {ex.Message}");
        }
    }

    public bool existe(ArchivoAnexoDTO archivoAnexoDTO){
        bool existe = false;
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"Select count(*) From Lito_jess.dbo.anexomov where ID=@id ";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", archivoAnexoDTO.Id);                    
                    int count = (int)command.ExecuteScalar();
                    existe = count > 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al verificar existencia de anexo: {ex.Message}");
        }
        return existe;

    }

    public void registrar(ArchivoAnexoDTO anexo)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {                
                connection.Open();
                string query = @"begin											                                    
				insert into Lito_jess.dbo.anexomov (Rama,Nombre,ID,Direccion,Icono,Tipo,Orden,Comentario,FechaEmision,TipoDocumento)
				            values(@rama,@nombreArchivo,@id,@path,66,'Archivo',1,'SCORREA',GETDATE(),@tipo);
		     	end	";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", anexo.Id);
                    command.Parameters.AddWithValue("@path", anexo.RutaArchivoDB);
                    command.Parameters.AddWithValue("@rama", "DIN");
                    command.Parameters.AddWithValue("@nombreArchivo", anexo.NombreArchivo);
                    command.Parameters.AddWithValue("@tipo", anexo.Tipo);
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al registrar archivo anexo: {ex.Message}");
        }
    }

}
