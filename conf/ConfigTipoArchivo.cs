using pdf_recorte.DTO;
using pdf_recorte.strategy;

namespace pdf_recorte.conf;

public class ConfigTipoArchivo
{
    public string TextoInicio { get; }
    public string TextoFin { get; }
    public float MargenArriba { get; }

  public Func<string, string, SearchStrategy> CrearEstrategia { get; }

    private ConfigTipoArchivo(string textoInicio, string textoFin, float margenArriba, Func<string, string, SearchStrategy> crearEstrategia)
    {
        TextoInicio = textoInicio;
        TextoFin = textoFin;
        MargenArriba = margenArriba;
        CrearEstrategia = crearEstrategia;
    }

    private static readonly Dictionary<TipoArchivo, ConfigTipoArchivo> _configuraciones = new()
    {
        [TipoArchivo.PLATAFORMA] = new ConfigTipoArchivo(
            "Servicio Integral de Tesoreria (SIT)",
            "_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _",
            20f,
            (inicio, fin) => new BuscadorStrategy(inicio, fin)
        ),
        [TipoArchivo.CASH] = new ConfigTipoArchivo(
            "BBVA Net Cash",
            "www.bbvanetcash.mx",
            30f,
            (inicio, fin) => new SearchStrategy(inicio, fin)
        )
    };

    public static ConfigTipoArchivo? ObtenerPorTipo(TipoArchivo tipo)
    {
        return _configuraciones.TryGetValue(tipo, out var config) ? config : null;
    }

}
