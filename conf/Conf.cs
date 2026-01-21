using System;

namespace pdf_recorte.conf;
using DotNetEnv;
public class Conf
{
    private static readonly object _lock = new();

    public string Environment { get; set; } 
    public string ConnectionString { get; set; }
    public string HotFolderPath { get; set; }
    private  static Conf? _instance ;

    public string BasePathDestino { get; set; } = "recortados";

    private Conf()
    {
        Env.Load();
        Environment = Env.GetString("ENVIRONMENT") ?? throw new InvalidOperationException("La variable de entorno 'ENVIRONMENT' no está definida.");
        bool isDevelopment = Environment.Equals("development", StringComparison.OrdinalIgnoreCase);        
        Console.WriteLine($"Entorno: {(isDevelopment ? "Desarrollo" : "Producción")}");
        ConnectionString = Env.GetString("SQL_CONNECTION_STRING") ?? throw new InvalidOperationException("La variable de entorno 'SQL_CONNECTION_STRING' no está definida.");
        HotFolderPath = Env.GetString($"HOT_FOLDER_PATH_{(isDevelopment ? "DEV" : "PROD")}") ?? throw new InvalidOperationException("La variable de entorno 'HOT_FOLDER_PATH' no está definida."); 
    }

    public static Conf getInstance()
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new Conf();
                }
            }
        }
        return _instance;
    }
}