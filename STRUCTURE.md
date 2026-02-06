# 📖 Estructura y Arquitectura del Proyecto

Este documento explica en detalle cómo funciona cada componente del proyecto.

## 🔄 Flujo de una Petición

```
1. Usuario accede a /Wallets
         ↓
2. Router mapea a WalletsController.Index()
         ↓
3. Controller llama a _bitpandaService.GetWalletsAsync()
         ↓
4. Service hace HTTP request a api.bitpanda.com
         ↓
5. Service deserializa JSON a modelos C#
         ↓
6. Service transforma a ViewModel
         ↓
7. Controller pasa ViewModel a View()
         ↓
8. Razor renderiza HTML con los datos
         ↓
9. HTML se envía al navegador
```

## 📁 Detalle de Carpetas

### Controllers/

Los controladores manejan las peticiones HTTP y devuelven respuestas.

```csharp
public class WalletsController : Controller
{
    // Campo privado - almacena la dependencia inyectada
    private readonly IBitpandaService _bitpandaService;

    // Constructor - recibe dependencias via DI
    public WalletsController(IBitpandaService bitpandaService)
    {
        _bitpandaService = bitpandaService;
    }

    // Acción - responde a GET /Wallets
    public async Task<IActionResult> Index()
    {
        var model = await _bitpandaService.GetWalletsAsync();
        return View(model);  // Busca Views/Wallets/Index.cshtml
    }
}
```

**Convenciones:**
- Nombre: `{Nombre}Controller.cs`
- Hereda de `Controller`
- Métodos públicos = Acciones
- `async Task<IActionResult>` para operaciones asíncronas

### Models/

Los modelos representan estructuras de datos.

**Response Models** - Mapean el JSON de la API:
```csharp
public class CryptoWalletData
{
    [JsonPropertyName("type")]      // Mapea "type" del JSON
    public string Type { get; set; }
    
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("attributes")]
    public CryptoWalletAttributes Attributes { get; set; }
}
```

**ViewModels** - Datos procesados para vistas:
```csharp
public class WalletsViewModel
{
    public List<WalletDisplayItem> CryptoWallets { get; set; }
    public decimal TotalValueEUR { get; set; }
    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
}
```

### Services/

La capa de servicios contiene la lógica de negocio.

**Interfaz (IBitpandaService.cs):**
```csharp
public interface IBitpandaService
{
    bool HasApiKey { get; }
    Task<TickerViewModel> GetTickerAsync();
    Task<WalletsViewModel> GetWalletsAsync();
}
```

**Implementación (BitpandaService.cs):**
```csharp
public class BitpandaService : IBitpandaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private string? _apiKey;

    public BitpandaService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<TickerViewModel> GetTickerAsync()
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync("https://api.bitpanda.com/v1/ticker");
        var json = await response.Content.ReadAsStringAsync();
        // Deserializar y transformar...
    }
}
```

### Views/

Las vistas usan Razor syntax (mezcla de HTML y C#).

**_Layout.cshtml** - Plantilla base:
```html
<!DOCTYPE html>
<html>
<head>
    <title>@ViewData["Title"]</title>
</head>
<body>
    <nav><!-- Navegación --></nav>
    
    @RenderBody()  <!-- Contenido de cada página -->
    
    <footer><!-- Footer --></footer>
</body>
</html>
```

**Página específica (Index.cshtml):**
```cshtml
@model WalletsViewModel

@if (Model.IsSuccess)
{
    @foreach (var wallet in Model.CryptoWallets)
    {
        <div>@wallet.Symbol: @wallet.Balance</div>
    }
}
else
{
    <div>Error: @Model.ErrorMessage</div>
}
```

## 🔧 Program.cs Explicado

```csharp
// 1. Crear el builder
var builder = WebApplication.CreateBuilder(args);

// 2. Registrar servicios en el contenedor DI
builder.Services.AddControllersWithViews();  // MVC
builder.Services.AddHttpClient();             // HttpClientFactory
builder.Services.AddSingleton<IBitpandaService, BitpandaService>();

// 3. Configurar el puerto
builder.WebHost.ConfigureKestrel(o => o.ListenAnyIP(3337));

// 4. Construir la aplicación
var app = builder.Build();

// 5. Configurar middleware
app.UseStaticFiles();    // Archivos de wwwroot
app.UseRouting();        // Sistema de rutas
app.UseAuthorization();  // Auth (no usado aquí)

// 6. Definir rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 7. Ejecutar
app.Run();
```

## 🎨 Ciclo de Vida DI

```
Startup:
  builder.Services.AddSingleton<IBitpandaService, BitpandaService>()
         ↓
Request:
  Controller necesita IBitpandaService
         ↓
  DI Container crea/devuelve instancia de BitpandaService
         ↓
  Constructor del Controller recibe la instancia
```

**Tipos de registro:**
- `Singleton`: Una instancia para toda la aplicación
- `Scoped`: Una instancia por request HTTP
- `Transient`: Nueva instancia cada vez que se solicita

## 📝 Razor Syntax

```cshtml
@* Comentario *@

@{ /* Bloque de código C# */ }

@variable              @* Imprimir valor *@

@Model.Property        @* Acceder al modelo *@

@if (condition) { }    @* Condicionales *@

@foreach (var x in list) { }  @* Loops *@

@Html.Raw(htmlString)  @* HTML sin escapar *@

<a asp-controller="Home" asp-action="Index">Link</a>  @* Tag Helper *@
```

## 🔐 Manejo de API Key

```csharp
// 1. Servicio almacena en memoria
private string? _apiKey;

public void SetApiKey(string apiKey)
{
    _apiKey = apiKey;
}

// 2. Se usa en requests
private HttpClient CreateClient(bool authenticated = false)
{
    var client = _httpClientFactory.CreateClient();
    
    if (authenticated && !string.IsNullOrEmpty(_apiKey))
    {
        client.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
    }
    
    return client;
}
```

## 📊 Transformación de Datos

```csharp
// JSON de la API
{
  "data": [{
    "attributes": {
      "cryptocoin_symbol": "BTC",
      "balance": "1.5"
    }
  }]
}

// Modelo de respuesta
public class CryptoWalletsResponse
{
    [JsonPropertyName("data")]
    public List<CryptoWalletData> Data { get; set; }
}

// Transformación a ViewModel
var displayItems = response.Data
    .Select(w => new WalletDisplayItem
    {
        Symbol = w.Attributes.CryptocoinSymbol,
        Balance = decimal.Parse(w.Attributes.Balance)
    })
    .ToList();
```

## 🧪 Testing (Futuro)

La estructura permite fácil testing:

```csharp
// Mock del servicio
public class MockBitpandaService : IBitpandaService
{
    public Task<TickerViewModel> GetTickerAsync()
    {
        return Task.FromResult(new TickerViewModel
        {
            Items = new List<TickerItem>
            {
                new() { Symbol = "BTC", PriceEUR = 50000 }
            }
        });
    }
}

// Test del controller
var controller = new TickerController(new MockBitpandaService());
var result = await controller.Index();
// Assert...
```

---

*Esta documentación es parte del proyecto educativo Bitpanda Explorer.*
