using System;
using pdf_recorte.DTO;

namespace pdf_recorte.DAO.interfaces;

public interface IAnexoDAO
{
    void  borrar(string id);     
    bool existe(ArchivoAnexoDTO archivoAnexoDTO);
    void registrar(ArchivoAnexoDTO anexo);
    ArchivoAnexoDTO? obtener(string monto, string numeroProveedor, string fechaOperacion);



}
