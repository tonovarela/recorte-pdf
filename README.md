# PDF-Recorte (iText 9, C# .NET 8)

Aplicación de consola que procesa PDFs de comprobantes bancarios, extrae recibos individuales y los registra en base de datos SQL Server. Identifica automáticamente:

- Número de operación / Folio de firma
- Número de proveedor
- Fecha de operación
- Monto del pago

## Tipos de Archivo Soportados

| Tipo | Descripción | Texto delimitador inicio |
|------|-------------|--------------------------|
| **PLATAFORMA** | Servicio Integral de Tesorería (SIT) | `Servicio Integral de Tesoreria (SIT)` |
| **CASH** | BBVA Net Cash | `BBVA Net Cash` |

## Requisitos

- .NET 8.0+
- SQL Server (para registro de anexos)
- Archivo `.env` con variables de configuración

## Dependencias

```xml
<PackageReference Include="DotNetEnv" Version="3.1.1" />
<PackageReference Include="itext" Version="9.5.0" />
<PackageReference Include="itext.bouncy-castle-adapter" Version="9.5.0" />
<PackageReference Include="Microsoft.Data.SqlClient" Version="7.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.5" />
```

## Configuración

Crear archivo `.env` en la raíz del proyecto:

```env
ENVIRONMENT=development
SQL_CONNECTION_STRING=Server=...;Database=...;User Id=...;Password=...
HOT_FOLDER_PATH_DEV=/ruta/local/entrada
HOT_FOLDER_PATH_PROD=/ruta/produccion/entrada
```

> **Nota macOS/Linux:** Las rutas de archivos se transforman automáticamente (reemplaza `192.168.2.217` por `Volumes`). El recurso de red debe estar montado.

## Arquitectura

```
pdf-recorte/
├── Program.cs                    # Punto de entrada, DI container
├── conf/
│   ├── Conf.cs                   # Configuración desde .env
│   └── ConfigTipoArchivo.cs      # Configuración por tipo de PDF
├── services/
│   ├── interfaces/
│   │   └── IProcesadorPDF.cs
│   └── implementation/
│       └── ProcesadorPDF.cs      # Orquestador principal
├── DAO/
│   ├── interfaces/
│   │   └── IAnexoDAO.cs
│   └── implementation/
│       └── AnexoDAO.cs           # Acceso a SQL Server
├── DTO/
│   ├── ReciboDTO.cs              # Datos del recibo extraído
│   ├── ArchivoAnexoDTO.cs        # Datos del anexo en BD
│   └── ArchivoClasificado.cs     # Archivo con tipo detectado
├── strategy/
│   ├── SearchStrategy.cs         # Estrategia base (CASH)
│   └── BuscadorStrategy.cs       # Estrategia PLATAFORMA/SIT
├── utils/
│   ├── ArchivoManager.cs         # Clasificación de archivos
│   └── PDFManager.cs             # Extracción y recorte de PDFs
└── entrada/                      # Hot folder de desarrollo
```

## Flujo de Ejecución

1. **Clasificación:** Escanea el hot folder y detecta el tipo de cada PDF
2. **Extracción:** Usa la estrategia correspondiente para extraer datos de cada recibo
3. **Búsqueda:** Consulta en BD para obtener información del anexo asociado
4. **Recorte:** Genera PDF individual por cada recibo
5. **Registro:** Actualiza/inserta registro en tabla `anexomov`

## Uso

```bash

# Desarrollo
dotnet run

# Compilar release
dotnet build -c Release

# Publicar
dotnet publish -c Release -o publish

# Copiar el .env en la carpeta publish o crearla 
# Antes debes se ingresar los datos requeridos
cp .env.development ./publish/.env
```




## Extracción de Datos por medio de expresiones regulares.

### PLATAFORMA (SIT)
- **Número de operación:** `Número de operación\s*([0-9\s]+)`
- **Número de proveedor:** `(?:Clave\s+del\s+Proveedor|PROVEEDOR)\s*([0-9\s]+)`
- **Fecha de operación:** `Fecha\s+de\s+operación\s*([0-9]{2}/[0-9]{2}/[0-9]{4})`

### CASH (BBVA Net Cash)
- **Folio de firma:** `Folio de firma:\s*([0-9\s]+)`
- **Fecha de aplicación:** `Fecha de aplicación:\s*([0-9]{2}/[0-9]{2}/[0-9]{4})`
- **Importe del pago:** `Importe del pago\s*([0-9,]+\.?[0-9]*)`

## .gitignore

- `bin/`, `obj/` - Artefactos de compilación
- `recortados/*` - PDFs generados
- `.env` - Variables de entorno (sensible)
- `entrada/*.pdf` - PDFs de entrada

## Notas

- Ajusta los patrones regex en `BuscadorStrategy` o `SearchStrategy` si el formato del comprobante cambia
- Si el PDF no contiene los textos delimitadores, no se generarán recortes para esa página
- La aplicación usa inyección de dependencias con `Microsoft.Extensions.DependencyInjection`