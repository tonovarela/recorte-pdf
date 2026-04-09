namespace pdf_recorte.conf;
using DotNetEnv;
public class Conf
{    
    public string Environment { get; set; } 
    public string ConnectionString { get; set; }
    public string HotFolderPath { get; set; }
    
    public Conf()
    {
        Env.Load();
        Console.WriteLine("Cargando configuración...");
        Environment = Env.GetString("ENVIRONMENT") ?? throw new InvalidOperationException("La variable de entorno 'ENVIRONMENT' no está definida.");
        bool isDevelopment = Environment.Equals("development", StringComparison.OrdinalIgnoreCase);        
        Console.WriteLine($"Entorno: {(isDevelopment ? "Desarrollo" : "Producción")}");
        ConnectionString = Env.GetString("SQL_CONNECTION_STRING") ?? throw new InvalidOperationException("La variable de entorno 'SQL_CONNECTION_STRING' no está definida.");        
        HotFolderPath = Env.GetString($"HOT_FOLDER_PATH_{(isDevelopment ? "DEV" : "PROD")}") ?? throw new InvalidOperationException("La variable de entorno 'HOT_FOLDER_PATH' no está definida.");             
    }

    
}