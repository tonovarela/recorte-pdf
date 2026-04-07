using System;

namespace pdf_recorte.DAO;

using Microsoft.Data.SqlClient;

public class OrdenDAO : DAO
{


    public void obtenerDatos(double monto, string contacto, string fechaEmision)
    {

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
                        while (reader.Read())
                        {
                            string? id = reader["ID"].ToString();
                            string? nombre = reader["Nombre"].ToString();
                            string? direccion = reader["Direccion"].ToString();
                            DateTime? fecha = reader["FechaEmision"] as DateTime?;                            
                            Console.WriteLine($"ID: {id}, Nombre: {nombre}, Dirección: {direccion}, Fecha de Emisión: {fecha}");
                        }
                    }
                }
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener datos: {ex.Message}");
        }

    }

    public void registrarArchivoAnexo(string rutaArchivo, int idOrden, string tipo)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                //DOC
                connection.Open();
                string query = @"begin				
				declare @nombreArchivo varchar(250);
				declare @path varchar(250);
                declare @rama varchar(5);
				declare @id int;
				set @id=@id_;
                set @rama=@rama_;
				set @path=@path_
			        set @nombreArchivo=@nombreArchivo_                                                               
			        declare @orden int;				
		 	        select @orden= iif(max(Orden) is null,0,max(Orden))  from dbo.anexomov			
                                where rama=@rama and ID=@id                                                
                            
				insert into dbo.anexomov (Rama,Nombre,ID,Direccion,Icono,Tipo,Orden,Comentario,FechaEmision,TipoDocumento)
				            values(@rama,@nombreArchivo,@id,@path,66,'Archivo',@orden+1,'HOT FOLDER',GETDATE(),@tipo);
		     	end	";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_", idOrden);
                    command.Parameters.AddWithValue("@path_", rutaArchivo);
                    command.Parameters.AddWithValue("@rama_", "CXP");
                    command.Parameters.AddWithValue("@nombreArchivo_", Path.GetFileName(rutaArchivo));
                    command.Parameters.AddWithValue("@tipo", tipo);
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
