using System;
using pdf_recorte.conf;

namespace pdf_recorte.DAO;

public class DAO
{
    public static string connectionString = String.Empty;

    public DAO()
    {
        Conf conf = Conf.getInstance();
        connectionString = conf.ConnectionString;
    }

}
