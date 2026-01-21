# Recorte de PDF (iText 7, C#)

Extrae bloques de comprobantes desde un PDF y genera archivos recortados por cada recibo, identificando:
- Cuenta del proveedor
- Número de operación
- Número de proveedor
- Fecha de operación

## Requisitos
- .NET 6+ (o compatible)
- iText 7 (Kernel y extracción de texto)

## Estructura clave
- Program.cs: orquestación, lectura del PDF y generación de recortes.
- BuscadorStrategy.cs: extracción de texto y parseo con expresiones regulares.
- DTO/ReciboDTO.cs: datos del recorte y nombre destino.

## Uso
1. Coloca el PDF de entrada: `comprobante.pdf` en la raíz del proyecto.
2. Ejecuta:
   - macOS/Linux:
     ```bash
     dotnet run
     ```
3. Los archivos recortados se guardan bajo `recortados/` con nombre derivado de los datos de cada recibo.

## Configuración de salida
- Base de salida: `recortados` (ver `_basePathDestino` en Program.cs).
- Cada `ReciboDTO` define `pathDestinoIndividual()` para su archivo.



## .gitignore
- Se excluyen binarios (`bin/`, `obj/`), artefactos de IDE y PDFs generados (`recortados/*`).
- `comprobante.pdf` está ignorado por defecto para evitar subir datos sensibles.

## Notas
- Ajusta los patrones regex en `BuscadorStrategy` si el formato del comprobante cambia.
- Si el PDF no contiene los textos delimitadores, no se generarán recortes para esa página.