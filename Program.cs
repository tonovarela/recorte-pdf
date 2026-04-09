

using pdf_recorte.conf;
using pdf_recorte.DAO.implementation;
using pdf_recorte.services.interfaces;
using pdf_recorte.services.implementation;
using Microsoft.Extensions.DependencyInjection;
using pdf_recorte.DAO.interfaces;


public partial class Program
{


    public static void Main(string[] args)
    {    
        var services = new ServiceCollection();                
        services.AddSingleton<Conf>();        
        services.AddScoped<IAnexoDAO, AnexoDAO>();
        services.AddScoped<IProcesadorPDF, ProcesadorPDF>();

        var provider = services.BuildServiceProvider();
        var procesador = provider.GetRequiredService<IProcesadorPDF>();
        procesador.Ejecutar();

    }


}
