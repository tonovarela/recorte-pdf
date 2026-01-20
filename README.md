# Recorte de PDF por delimitadores (iText 7 C#)

Proyecto de consola .NET 8 que:
- Busca bloques en un PDF desde el texto de inicio "Servicio Integral de Tesoreria (SIT)" hasta el delimitador de guiones bajos.
- Genera un PDF independiente por cada bloque detectado.
- Soporta múltiples bloques por página.

## Requisitos
- .NET SDK 8.0
- iText 7 (referencias ya incluidas en `bin/` por la compilación).

## Estructura
- `Program.cs`: Lógica principal. Copia la página al nuevo documento y ajusta MediaBox/CropBox para el recorte.
- `BuscadorStrategy.cs`: Estrategia de extracción que detecta rectángulos de inicio y fin.
- `comprobante.pdf`: PDF de entrada (no se versiona por defecto).
- `recorte_resultado_*.pdf`: PDFs generados (ignorados por git).

## Uso
1. Coloca `comprobante.pdf` en la raíz del proyecto.
2. Ajusta los textos si es necesario en `Program.cs`:
   - `textoInicio = "Servicio Integral de Tesoreria (SIT)"`
   - `textoFin = "_ _ _ _ _ _ ..."` (cadena de delimitadores)
3. Ejecuta:
   - `dotnet build`
   - `dotnet run`
4. Revisa los archivos `recorte_resultado_1.pdf`, `recorte_resultado_2.pdf`, etc.

## Notas
- Coordenadas: iText usa origen en esquina inferior izquierda.
- Márgenes del recorte: ajusta los valores en `Program.cs` (margenIzq, margenAbajo, margenDer, margenArriba).
- Si un bloque queda fuera de la `MediaBox`, se omite.
- Si tu visor muestra en blanco, prueba solo `SetCropBox` (comentando `SetMediaBox`).

## Git
- `.gitignore` incluye:
  - `bin/`, `obj/`, artefactos .NET
  - `recorte_resultado_*.pdf`
  - `comprobante.pdf` (opcional)
  - `.DS_Store`

## Licencia
Este proyecto usa iText Core 9.x (AGPL). Asegúrate de cumplir la licencia de iText para tu caso de uso.
