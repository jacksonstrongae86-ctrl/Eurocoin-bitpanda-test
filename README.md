# 🚀 Bitpanda Explorer

Proyecto educativo en **C# .NET 8** que demuestra cómo integrar y visualizar la API de Bitpanda.

## 📍 Acceso

**URL:** http://76.13.136.20:3337

## 🏗️ Estructura del Proyecto

```
bitpanda-explorer/
├── BitpandaExplorer.sln          # Solución de Visual Studio
├── README.md                      # Este archivo
├── STRUCTURE.md                   # Documentación detallada
│
└── BitpandaExplorer/             # Proyecto principal
    ├── Program.cs                 # Punto de entrada + DI config
    ├── appsettings.json          # Configuración
    │
    ├── Controllers/              # Controladores MVC
    │   ├── HomeController.cs     # Dashboard + Settings
    │   ├── TickerController.cs   # Precios (público)
    │   ├── WalletsController.cs  # Wallets (requiere API Key)
    │   ├── TradesController.cs   # Trades (requiere API Key)
    │   └── TransactionsController.cs
    │
    ├── Models/                   # Modelos de datos
    │   ├── Ticker/              # Modelos para precios
    │   │   └── TickerResponse.cs
    │   ├── Wallet/              # Modelos para wallets
    │   │   └── WalletModels.cs
    │   ├── Trade/               # Modelos para trades
    │   │   └── TradeModels.cs
    │   └── Transaction/         # Modelos para transacciones
    │       └── TransactionModels.cs
    │
    ├── Services/                 # Capa de servicios
    │   ├── IBitpandaService.cs  # Interfaz (contrato)
    │   └── BitpandaService.cs   # Implementación
    │
    └── Views/                    # Vistas Razor
        ├── _ViewImports.cshtml
        ├── _ViewStart.cshtml
        ├── Shared/
        │   └── _Layout.cshtml   # Layout principal
        ├── Home/
        │   ├── Index.cshtml     # Dashboard
        │   ├── Settings.cshtml  # Configuración API Key
        │   └── Docs.cshtml      # Documentación
        ├── Ticker/
        │   └── Index.cshtml     # Lista de precios
        ├── Wallets/
        │   └── Index.cshtml     # Lista de wallets
        ├── Trades/
        │   └── Index.cshtml     # Historial de trades
        └── Transactions/
            └── Index.cshtml     # Historial de transacciones
```

## 🎯 Páginas Disponibles

| Ruta | Descripción | Auth |
|------|-------------|------|
| `/` | Dashboard principal | No |
| `/Ticker` | Precios de todos los activos | No |
| `/Wallets` | Tus wallets crypto y fiat | Sí |
| `/Trades` | Historial de compras/ventas | Sí |
| `/Transactions` | Depósitos, retiros, etc. | Sí |
| `/Home/Settings` | Configurar API Key | No |
| `/Home/Docs` | Documentación del proyecto | No |

## 🔑 Conceptos de .NET Demostrados

### 1. Dependency Injection (DI)
```csharp
// Program.cs - Registro
builder.Services.AddSingleton<IBitpandaService, BitpandaService>();

// Controller - Inyección
public HomeController(IBitpandaService service) { }
```

### 2. Patrón MVC
- **Model:** Clases en `/Models` que representan datos
- **View:** Archivos `.cshtml` con Razor syntax
- **Controller:** Clases en `/Controllers` que manejan requests

### 3. Async/Await
```csharp
public async Task<IActionResult> Index()
{
    var data = await _service.GetDataAsync();
    return View(data);
}
```

### 4. HttpClient Factory
```csharp
builder.Services.AddHttpClient();
// En servicio:
var client = _httpClientFactory.CreateClient();
```

### 5. Interfaces
```csharp
public interface IBitpandaService
{
    Task<TickerViewModel> GetTickerAsync();
}

public class BitpandaService : IBitpandaService
{
    // Implementación
}
```

## 📡 API Endpoints de Bitpanda

| Endpoint | Auth | Descripción |
|----------|------|-------------|
| `GET /ticker` | No | Precios actuales |
| `GET /wallets` | API Key | Wallets crypto |
| `GET /fiatwallets` | API Key | Wallets fiat |
| `GET /trades` | API Key | Historial trades |
| `GET /wallets/transactions` | API Key | Transacciones |

**Base URL:** `https://api.bitpanda.com/v1`

## 🛠️ Comandos

```bash
# Compilar
dotnet build

# Ejecutar
dotnet run

# Publicar para producción
dotnet publish -c Release
```

## 📂 Archivos Clave

### Program.cs
Punto de entrada. Configura:
- Dependency Injection
- Middleware pipeline
- Routing
- Puerto (3337)

### Services/BitpandaService.cs
Toda la lógica de comunicación con la API:
- Manejo de HttpClient
- Serialización JSON
- Caché del ticker
- Manejo de errores

### Views/Shared/_Layout.cshtml
Layout HTML común:
- Navegación
- Estilos CSS
- Footer
- Bootstrap 5

## 🔒 Seguridad

- API Key se almacena solo en memoria (sesión)
- No se persiste en disco
- Protección CSRF en formularios
- Uso de HTTPS recomendado en producción

## 📚 Recursos

- [Documentación Bitpanda API](https://developers.bitpanda.com/platform/)
- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core)
- [C# Guide](https://docs.microsoft.com/dotnet/csharp/)

---

*Proyecto educativo - Matias Jackson / Unity 2026*
