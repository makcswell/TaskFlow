# EBCEYS.Infrastructure

[![CI / CD](https://github.com/EBCEYS/EBCEYS.Infrastructure/actions/workflows/dotnet-master.yml/badge.svg?branch=main)](https://github.com/EBCEYS/EBCEYS.Infrastructure/actions/workflows/dotnet-master.yml)
[![Deploy Documentation](https://github.com/EBCEYS/EBCEYS.Infrastructure/actions/workflows/docs.yml/badge.svg?branch=main)](https://github.com/EBCEYS/EBCEYS.Infrastructure/actions/workflows/docs.yml)
[![Coverage](https://ebceys.github.io/EBCEYS.Infrastructure/badges/badge_linecoverage.svg)](https://ebceys.github.io/EBCEYS.Infrastructure/badges/index.html)

Комплексная инфраструктурная библиотека на C#, предоставляющая готовые утилиты и компоненты для построения надёжных
ASP.NET Core приложений.

[Кодовая документация](https://ebceys.github.io/EBCEYS.Infrastructure/index.html)

---

## Навигация

- [EBCEYS.Infrastructure](#ebceysinfrastructure)
    - [Навигация](#навигация)
    - [Установка](#установка)
    - [Быстрый старт](#быстрый-старт)
    - [Компоненты](#компоненты)
        - [Bootstrapping](#bootstrapping)
            - [WebApplicationBase](#webapplicationbase)
            - [ExtraStartupBase](#extrastartupbase)
            - [ConfiguredApp](#configuredapp)
        - [Исключения](#исключения)
            - [ApiException](#apiexception)
            - [ApiExceptionHelper](#apiexceptionhelper)
        - [Фильтры контроллеров](#фильтры-контроллеров)
            - [ApiExceptionFilter](#apiexceptionfilter)
        - [Middlewares](#middlewares)
            - [RequestLoggingMiddleware](#requestloggingmiddleware)
            - [RequestMetricsMiddleware](#requestmetricsmiddleware)
            - [ExceptionCatcherMiddleware](#exceptioncatchermiddleware)
        - [Атрибуты](#атрибуты)
            - [NoRequestBodyLoggingAttribute](#norequestbodyloggingattribute)
            - [NoResponseBodyLoggingAttribute](#noresponsebodyloggingattribute)
        - [Паттерн Command](#паттерн-command)
            - [ICommand](#icommand)
            - [ICommandExecutor / CommandExecutor](#icommandexecutor--commandexecutor)
            - [IScopedCommandExecutor / ScopedCommandExecutor](#iscopedcommandexecutor--scopedcommandexecutor)
        - [HTTP-клиент](#http-клиент)
            - [ClientBase](#clientbase)
            - [OperationResult](#operationresult)
            - [ClientRegistrationExtensions](#clientregistrationextensions)
            - [IClientTokenManager](#iclienttokenmanager)
            - [DefaultClientPollyHelper](#defaultclientpollyhelper)
            - [ServiceSystemClient](#servicesystemclient)
        - [JWT](#jwt)
            - [IJwtGenerator / JwtGenerator](#ijwtgenerator--jwtgenerator)
            - [IJwtValidator / JwtValidator](#ijwtvalidator--jwtvalidator)
            - [JwtOptions](#jwtoptions)
            - [JwtObjectsExtensions](#jwtobjectsextensions)
        - [Атомарные генераторы последовательностей](#атомарные-генераторы-последовательностей)
            - [IAtomGenerator](#iatomgenerator)
            - [AtomicLongGenerator / AtomicIntGenerator](#atomiclonggenerator--atomicintgenerator)
        - [База данных](#база-данных)
            - [DbContextRegistrationExtensions](#dbcontextregistrationextensions)
            - [MigrationApplierService](#migrationapplierservice)
            - [DateTimeOffsetConverter](#datetimeoffsetconverter)
        - [RabbitMQ](#rabbitmq)
            - [EbRabbitMqClient](#ebrabbitmqclient)
            - [RabbitMqRegistrationExtensions](#rabbitmqregistrationextensions)
            - [SimpleRabbitMqConfiguration](#simplerabbitmqconfiguration)
            - [RabbitMqExtensions](#rabbitmqextensions)
        - [Планировщик задач](#планировщик-задач)
            - [SchedulingExtensions](#schedulingextensions)
        - [Сервисы до запуска хоста](#сервисы-до-запуска-хоста)
            - [IBeforeHostingStartedService](#ibeforehostingstartedservice)
        - [Service Controller](#service-controller)
        - [Модели](#модели)
            - [ServiceApiInfo](#serviceapiinfo)
            - [ProblemDetailsResult](#problemdetailsresult)
        - [Swagger](#swagger)
            - [ConfigureSwaggerOptions](#configureswaggeroptions)
        - [Расширения](#расширения)
            - [EbObjectExtensions](#ebobjectextensions)
            - [EbServerExtensions](#ebserverextensions)
            - [EbIServiceCollectionExtensions](#ebiservicecollectionextensions)
    - [Тестовая инфраструктура](#тестовая-инфраструктура)
    - [Лицензия](#лицензия)
    - [Автор](#автор)

---

## Установка

Добавьте ссылку на проект:

```bash
dotnet add reference ./Ebceys.Infrastructure.csproj
```

Или через NuGet (если опубликовано):

```bash
dotnet add package EBCEYS.Infrastructure
```

---

## Быстрый старт

Основной сценарий — создать класс Startup, унаследовав его от `ExtraStartupBase`, и запустить приложение через
`WebApplicationBase`:

```csharp
// Startup.cs
public class Startup(IConfiguration configuration) : ExtraStartupBase(configuration)
{
    protected override ServiceApiInfo ServiceApiInfo { get; init; } =
        new("MyService", "/api/v1", "Мой сервис");

    protected override void ServicesConfiguration(IServiceCollection services)
    {
        // регистрация ваших сервисов
    }

    protected override void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
    {
        // ваши middleware
    }
}

// Program.cs
await WebApplicationBase<Startup>
    .Create(args)
    .Build(args)
    .BuildAndRunAsync();
```

---

## Компоненты

---

### Bootstrapping

#### WebApplicationBase

`WebApplicationBase<TStartup>` — точка входа для построения приложения. Создаёт `WebApplication` на базе `TStartup`,
который
должен наследоваться от `ExtraStartupBase`. Использует `WebApplication.CreateBuilder` + `UseStartup<TStartup>()`.

```csharp
// Program.cs
await WebApplicationBase<Startup>
    .Create(args)
    .Build(args)
    .BuildAndRunAsync(token: cancellationToken);
```

---

#### ExtraStartupBase

Абстрактный базовый класс для Startup. Берёт на себя всю "скучную" инфраструктурную настройку:

- подключение Serilog
- FluentValidation с авто-валидацией
- Swagger + API versioning
- Prometheus-метрики
- Health Checks (PostgreSQL, RabbitMQ, ApplicationStatus)
- CORS, ProblemDetails, JWT-аутентификацию
- регистрацию `CommandExecutor` / `ScopedCommandExecutor`

**Переопределяемые свойства:**

| Свойство             | Описание                                      |
|----------------------|-----------------------------------------------|
| `UseAuthentication`  | Подключить JWT-аутентификацию                 |
| `ProxyToken`         | Прокси-режим токена (не валидировать locally) |
| `HealthCheckPort`    | Порт для health-check эндпоинтов              |
| `ServiceApiInfo`     | Метаданные сервиса (имя, base path, описание) |
| `HttpContextLogging` | Настройка параметров логирования HTTP         |

**Переопределяемые методы:**

| Метод                             | Описание                           |
|-----------------------------------|------------------------------------|
| `ServicesConfiguration(services)` | Регистрация ваших DI-сервисов      |
| `ConfigureMiddlewares(app, env)`  | Настройка кастомных middleware     |
| `ConfigureFilters(filters)`       | Добавление кастомных фильтров MVC  |
| `ConfigureHealthChecks(builder)`  | Добавление кастомных health checks |

```csharp
public class Startup(IConfiguration configuration) : ExtraStartupBase(configuration)
{
    protected override bool UseAuthentication { get; init; } = true;
    protected override int? HealthCheckPort { get; init; } = 8081;

    protected override ServiceApiInfo ServiceApiInfo { get; init; } =
        new("OrderService", "/order", "Сервис заказов");

    protected override void ServicesConfiguration(IServiceCollection services)
    {
        services.AddScopedCommand<CreateOrderCommand, CreateOrderContext, Order>();
        services.RegisterDbContext<AppDbContext>(opts =>
        {
            opts.ConnectionString = "Host=localhost;Database=orders;...";
            opts.MigrateDb = true;
        });
    }

    protected override void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
    {
        // свои middleware здесь
    }
}
```

---

#### ConfiguredApp

Обёртка над `IHost`, которую возвращает `WebApplicationBase.Build()`. Запускает все `IBeforeHostingStartedService`
перед стартом хоста.

> **Примечание:** `ServiceProvider` доступен только **после** вызова `BuildAndRunAsync()`.
> Обращение к нему до построения приложения выбросит `InvalidOperationException`.

```csharp
var app = WebApplicationBase<Startup>.Create(args).Build(args);

// Дополнительная конфигурация источников конфигурации
await app.BuildAndRunAsync(configureConf: builder =>
{
    builder.AddJsonFile("extra-settings.json", optional: true);
});
```

---

### Исключения

#### ApiException

Исключение для передачи HTTP-ответных ошибок в формате `ProblemDetails` через слои приложения. `ApiExceptionFilter`
перехватывает его и автоматически формирует правильный HTTP-ответ.

```csharp
// Выброс с кодом и сообщением
throw new ApiException(StatusCodes.Status400BadRequest, "Неверные данные запроса");

// Выброс с готовым ProblemDetails
throw new ApiException(new ProblemDetails
{
    Status = StatusCodes.Status403Forbidden,
    Title = "Нет доступа",
    Detail = "У вас нет прав для выполнения этого действия"
}, "Forbidden");

// Обёртка внутреннего исключения
throw new ApiException(StatusCodes.Status500InternalServerError, innerException);
```

---

#### ApiExceptionHelper

Статический вспомогательный класс для удобного выброса типизированных `ApiException`. Все методы автоматически
подставляют имя вызывающего метода через `[CallerMemberName]`.

Доступные методы:

| Метод                                                         | HTTP-статус        |
|---------------------------------------------------------------|--------------------|
| `ThrowNotFound<TResult>(message)`                             | 404 Not Found      |
| `ThrowNotFound<TResult, TException>(exception)`               | 404 Not Found      |
| `ThrowConflict<TResult>(message)`                             | 409 Conflict       |
| `ThrowConflict<TResult, TException>(exception)`               | 409 Conflict       |
| `ThrowValidation<TResult>(ValidationProblemDetails)`          | 422 Unprocessable  |
| `ThrowException<TResult, TException>(ex, statusCode?)`        | произвольный       |
| `ThrowException<TException>(ex, statusCode?)`                 | произвольный       |
| `ThrowApiException<TResult>(problemDetails, statusCode?)`     | произвольный       |
| `ThrowApiException(problemDetails, statusCode?)`              | произвольный       |
| `ThrowApiException<TResult, TError>(OperationResult<TError>)` | из OperationResult |

```csharp
public async Task<Order> GetOrderAsync(Guid id)
{
    var order = await _repo.FindAsync(id);
    if (order is null)
        return ApiExceptionHelper.ThrowNotFound<Order>($"Заказ {id} не найден");

    return order;
}

public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
{
    if (await _repo.ExistsAsync(dto.ExternalId))
        return ApiExceptionHelper.ThrowConflict<Order>("Заказ с таким ID уже существует");

    // ...
}
```

---

### Фильтры контроллеров

#### ApiExceptionFilter

MVC-фильтр исключений (`ExceptionFilterAttribute`). Автоматически регистрируется в `ExtraStartupBase`. Перехватывает:

- `ApiException` — возвращает `ProblemDetails` с кодом из исключения
- Любое другое исключение — возвращает `ProblemDetails` с кодом 500

Логирует ошибку перед формированием ответа.

```csharp
// Регистрируется автоматически через ExtraStartupBase.
// При необходимости можно зарегистрировать вручную:
services.AddControllers(opts =>
{
    opts.Filters.Add<ApiExceptionFilter>();
});
```

---

### Middlewares

#### RequestLoggingMiddleware

Middleware для логирования входящих HTTP-запросов и исходящих ответов. Логирует URL, метод, статус-код, время обработки
и (при соответствующем уровне логирования) тела запроса/ответа.

Автоматически исключает из логирования: `/swagger/`, `/metrics`, `/service/ping`, `/service/healthz`.

**`HttpLoggingOptions`** — класс настройки:

| Свойство                     | Описание                                                      |
|------------------------------|---------------------------------------------------------------|
| `PathStartExcludeLogging`    | Пути, начинающиеся с указанных строк — не логируются          |
| `PathContainsExcludeLogging` | Пути, содержащие указанные строки — не логируются             |
| `PathEndExcludeLogging`      | Пути, заканчивающиеся на указанные строки — не логируются     |
| `LogLevelToLogBodies`        | Уровень логирования тел запроса/ответа (по умолчанию `Debug`) |
| `LoggingContentTypes`        | Дополнительные content-type для логирования тел               |

```csharp
// В ExtraStartupBase:
protected override Action<HttpLoggingOptions>? HttpContextLogging => opts =>
{
    opts.PathStartExcludeLogging = ["/health"];
    opts.PathContainsExcludeLogging = ["/internal/"];
    opts.LogLevelToLogBodies = LogLevel.Trace;
};
```

---

#### RequestMetricsMiddleware

Middleware для сбора Prometheus-метрик по HTTP-запросам. Счётчик `prometheus_demo_request_total` разбит по меткам
`path`, `method`, `status`. Автоматически подключается через `ExtraStartupBase`.

---

#### ExceptionCatcherMiddleware

Перехватывает все необработанные исключения (кроме `ApiException`) и перебрасывает их как `ApiException` с кодом 500.
Это гарантирует, что любая необработанная ошибка всегда будет представлена в формате `ProblemDetails`.

---

### Атрибуты

#### NoRequestBodyLoggingAttribute

Декоратор для метода контроллера. Отключает логирование тела **запроса** для данного эндпоинта. Полезно для эндпоинтов,
принимающих чувствительные данные (пароли, токены).

```csharp
[HttpPost("login")]
[NoRequestBodyLogging]
public IActionResult Login([FromBody] LoginRequest request)
{
    // тело запроса не попадёт в логи
}
```

---

#### NoResponseBodyLoggingAttribute

Декоратор для метода контроллера. Отключает логирование тела **ответа**.

```csharp
[HttpGet("secret")]
[NoResponseBodyLogging]
public IActionResult GetSecret()
{
    // тело ответа не попадёт в логи
}
```

---

### Паттерн Command

#### ICommand

Базовый интерфейс команды. Реализуйте его для каждой бизнес-операции.

```csharp
public record CreateOrderContext(Guid CustomerId, List<OrderItem> Items);

public class CreateOrderCommand(IOrderRepository repo) : ICommand<CreateOrderContext, Order>
{
    public async Task<Order> ExecuteAsync(CreateOrderContext context, CancellationToken token = default)
    {
        var order = new Order(context.CustomerId, context.Items);
        await repo.AddAsync(order, token);
        return order;
    }
}
```

Регистрация в DI:

```csharp
// В ServicesConfiguration:
services.AddScopedCommand<CreateOrderCommand, CreateOrderContext, Order>();
// или как singleton:
services.AddSingletonCommand<CreateOrderCommand, CreateOrderContext, Order>();
// или как transient:
services.AddTransientCommand<CreateOrderCommand, CreateOrderContext, Order>();
```

---

#### ICommandExecutor / CommandExecutor

Синглтон-сервис для выполнения команд без явного получения их из DI. Логирует вход/выход/ошибку для каждой команды.

```csharp
public class OrderController(ICommandExecutor executor) : ControllerBase
{
    [HttpPost]
    public async Task<Order> CreateOrder([FromBody] CreateOrderContext context, CancellationToken ct)
    {
        return await executor.ExecuteCommandAsync<CreateOrderContext, Order>(context, ct);
    }
}
```

---

#### IScopedCommandExecutor / ScopedCommandExecutor

Scoped-версия `ICommandExecutor`. Используйте, когда команда должна выполняться в рамках текущего DI-скоупа (например,
внутри фонового сервиса, где выполняется единичная транзакция).

```csharp
public class OrderProcessor(IScopedCommandExecutor executor)
{
    public async Task ProcessAsync(ProcessOrderContext ctx, CancellationToken ct)
    {
        var result = await executor.ExecuteAsync<ProcessOrderContext, ProcessResult>(ctx, ct);
    }
}
```

---

### HTTP-клиент

#### ClientBase

Абстрактный базовый класс для типизированных HTTP-клиентов. Построен на базе [Flurl](https://flurl.dev/) с поддержкой:

- автоматической авторизации через токен
- политик повторных запросов (Polly)
- типизированных ответов через `OperationResult<TResponse, TError>`
- скачивания файлов (`Stream`, `byte[]`)

Методы для каждого HTTP-метода (GET, POST, PUT, PATCH, DELETE):

| Метод                                        | Описание                          |
|----------------------------------------------|-----------------------------------|
| `GetJsonAsync<TResponse, TError>`            | GET с десериализацией тела ответа |
| `GetAsync<TError>`                           | GET без тела успешного ответа     |
| `GetStreamAsync<TError>`                     | GET → Stream                      |
| `GetRawAsync<TError>`                        | GET → byte[]                      |
| `PostAsync<TError>`                          | POST без тела запроса             |
| `PostJsonAsync<TRequest, TError>`            | POST с JSON-телом запроса         |
| `PostJsonAsync<TResponse, TError>`           | POST без тела, с ответом          |
| `PostJsonAsync<TRequest, TResponse, TError>` | POST с JSON-телом и ответом       |
| `PostStreamAsync<TError>`                    | POST со Stream-телом              |
| `PostStreamAsync<TResponse, TError>`         | POST со Stream-телом и ответом    |
| `PutJsonAsync<TRequest, TError>`             | PUT с JSON-телом                  |
| `PutJsonAsync<TRequest, TResponse, TError>`  | PUT с JSON-телом и ответом        |
| `DeleteAsync<TError>`                        | DELETE без тела ответа            |
| `DeleteJsonAsync<TResponse, TError>`         | DELETE с телом ответа             |

```csharp
public interface IProductClient
{
    Task<Product?> GetProductAsync(Guid id, CancellationToken ct = default);
}

public class ProductClient(
    IFlurlClientCache cache,
    ILoggerFactory loggerFactory,
    ClientBaseUrlResolver baseUrlResolver)
    : ClientBase(cache, loggerFactory, baseUrlResolver), IProductClient
{
    public async Task<Product?> GetProductAsync(Guid id, CancellationToken ct = default)
    {
        var result = await GetJsonAsync<Product, ProblemDetails>(
            url => url.AppendPathSegments("products", id),
            token: ct);

        if (!result.IsSuccess)
            ApiExceptionHelper.ThrowNotFound<Product>($"Продукт {id} не найден");

        return result.Result;
    }
}
```

---

#### OperationResult

Структура результата HTTP-запроса `ClientBase`. Содержит либо успешный результат, либо ошибку.

```csharp
OperationResult<Product, ProblemDetails> result = await client.GetJsonAsync<Product, ProblemDetails>(...);

if (result.IsSuccess)
{
    Console.WriteLine(result.Result!.Name);
}
else
{
    Console.WriteLine($"Ошибка {result.StatusCode}: {result.Error?.Detail}");
}
```

---

#### ClientRegistrationExtensions

Fluent-API для регистрации `ClientBase`-клиентов в DI.

```csharp
// В ServicesConfiguration:
services.AddClient<IProductClient, ProductClient>()
    .FromUrl("http://product-service:8080")       // или .FromConfiguration(config, "ProductService")
    .AddAuthTokenFromHttpContextResolver()          // прокси токена из текущего запроса
    .Register();

// Конфигурация через appsettings.json:
// "ProductService": { "ServiceUrl": "http://product-service:8080" }
services.AddClient<IProductClient, ProductClient>()
    .FromConfiguration(configuration, "ProductService")
    .Register();
```

---

#### IClientTokenManager

Интерфейс для получения токена авторизации при исходящих запросах из `ClientBase`.

Встроенная реализация `FromContextClientTokenManager<TInterface>` берёт токен из заголовка `Authorization` текущего
входящего HTTP-запроса (прокси-режим).

```csharp
// Собственная реализация (например, получение service-to-service токена):
public class MyTokenManager : IClientTokenManager<IProductClient>
{
    public async Task<string?> GetTokenAsync()
    {
        return await _authService.GetServiceTokenAsync();
    }
}

// Регистрация:
services.AddSingleton<IClientTokenManager<IProductClient>, MyTokenManager>();
```

---

#### DefaultClientPollyHelper

Утилита для создания политики повторных запросов Polly при HTTP-ошибках (`FlurlHttpException`).

```csharp
IAsyncPolicy<IFlurlResponse> policy = DefaultClientPollyHelper.CreateDefaultHttpPolly<IFlurlResponse>(
    delay: TimeSpan.FromSeconds(2),
    onRetryAction: (ex, span, retry, ctx) =>
        logger.LogWarning("Retry {retry} after {span}: {msg}", retry, span, ex.Message));
```

---

#### ServiceSystemClient

Готовый клиент для обращения к системным эндпоинтам другого сервиса (тем, что регистрирует `ServiceController`).

```csharp
var client = new ServiceSystemClient(
    apiInfo,
    flurlCache,
    loggerFactory,
    baseUrlResolver: () => "http://other-service:8080");

await client.PingAsync();
await client.HealthCheckAsync();
var report = await client.HealthStatusCheckAsync();
var metrics = await client.GetMetricsAsync();
```

---

### JWT

#### IJwtGenerator / JwtGenerator

Генерация JWT-токенов на базе симметричного ключа (HMAC-SHA256). Настраивается через `JwtOptions`.

```csharp
// Инжектируется из DI (регистрируется автоматически при UseAuthentication = true):
public class AuthController(IJwtGenerator jwtGenerator) : ControllerBase
{
    [HttpPost("token")]
    public IActionResult GetToken(string userId, string role)
    {
        var token = jwtGenerator.GenerateKey(
            userId.ToClaim(ClaimTypes.NameIdentifier),
            role.ToClaim(ClaimTypes.Role));

        return Ok(new { token });
    }
}
```

---

#### IJwtValidator / JwtValidator

Парсинг и базовая валидация (синтаксис) JWT-токена без проверки подписи. Полезно для чтения claims из токена без полной
верификации (например, в proxy-режиме).

```csharp
// Регистрация в DI:
services.AddJwtValidator();

// Использование:
public class TokenReader(IJwtValidator validator)
{
    public string? GetUserId(string token)
    {
        if (!validator.TryValidate(token, out var jwt))
            return null;

        return jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    }
}
```

---

#### JwtOptions

POCO-класс конфигурации JWT. Биндится из секции `JwtOptions` в `appsettings.json`.

```json
{
    "JwtOptions": {
        "Issuer": "my-service",
        "Audience": "my-clients",
        "Base64Key": "BASE64_ENCODED_SECRET_KEY_MIN_32_BYTES",
        "TokenTimeToLive": "01:00:00"
    }
}
```

---

#### JwtObjectsExtensions

Метод расширения для удобного создания `Claim` из строки.

```csharp
var claim = userId.ToClaim(ClaimTypes.NameIdentifier);
// эквивалентно: new Claim(ClaimTypes.NameIdentifier, userId)
```

---

### Атомарные генераторы последовательностей

#### IAtomGenerator

Интерфейс потокобезопасного атомарного счётчика. Реализации: `AtomicLongGenerator` и `AtomicIntGenerator`.

```csharp
// Регистрация:
services.AddAtomicGenerators(seed: 42); // seed необязателен

// Использование:
public class OrderNumberGenerator(IAtomGenerator<long> generator)
{
    public long NextOrderNumber() => generator.Next();
}
```

---

#### AtomicLongGenerator / AtomicIntGenerator

Конкретные реализации `IAtomGenerator<long>` и `IAtomGenerator<int>`. Используют `Interlocked.Increment` для
потокобезопасного инкремента. Начальное значение генерируется случайно (или задаётся через `seed`).

```csharp
var gen = new AtomicLongGenerator(seed: 0);
long id1 = gen.Next(); // 1
long id2 = gen.Next(); // 2
```

---

### База данных

#### DbContextRegistrationExtensions

Расширение для регистрации `DbContext` с PostgreSQL (Npgsql), автоматически:

- регистрирует `IDbContextFactory<TDbContext>`
- добавляет Health Check для PostgreSQL-соединения
- планирует применение миграций при старте (если `MigrateDb = true`)

```csharp
services.RegisterDbContext<AppDbContext>(opts =>
{
    opts.ConnectionString = "Host=localhost;Port=5432;Database=mydb;Username=user;Password=pass";
    opts.MigrateDb = true;
    opts.Retries = 3;
    opts.Timeout = TimeSpan.FromSeconds(30);
});
```

---

#### MigrationApplierService

Реализация `IBeforeHostingStartedService`, которая применяет EF Core-миграции до запуска HTTP-сервера. Автоматически
регистрируется через `RegisterDbContext`, если включена опция `MigrateDb`.

```csharp
// При необходимости — ручная регистрация:
services.AddBeforeHostingStarted<MigrationApplierService<AppDbContext>>();
```

---

#### DateTimeOffsetConverter

EF Core `ValueConverter` для корректного сохранения `DateTimeOffset` в PostgreSQL: преобразует в UTC при записи и
чтении.

```csharp
protected override void ConfigureConventions(ModelConfigurationBuilder builder)
{
    builder.Properties<DateTimeOffset>()
        .HaveConversion<DateTimeOffsetConverter>();
}
```

---

### RabbitMQ

#### EbRabbitMqClient

Абстрактный базовый класс для типизированных RabbitMQ-клиентов. Оборачивает `RabbitMQClient` из библиотеки
`EBCEYS.RabbitMQ` и реализует `IHostedService` для управления жизненным циклом.

Доступные методы отправки:

| Метод                                                | Описание                                     |
|------------------------------------------------------|----------------------------------------------|
| `SendMessageAsync<TMessage>(msg, method)`            | Отправить сообщение без ожидания ответа      |
| `SendMessageAsync(method)`                           | Отправить пустое сообщение                   |
| `SendRequestAsync<TRequest, TResponse>(req, method)` | Отправить запрос и дождаться ответа          |
| `SendRequestAsync<TResponse>(method)`                | Отправить запрос без тела и дождаться ответа |

```csharp
public interface INotificationClient
{
    Task SendEmailNotificationAsync(EmailMessage message, CancellationToken ct = default);
}

public class NotificationClient(ILogger<RabbitMQClient> logger, RabbitMQConfiguration config)
    : EbRabbitMqClient(logger, config), INotificationClient
{
    public Task SendEmailNotificationAsync(EmailMessage message, CancellationToken ct = default)
        => SendMessageAsync(message, "send-email", token: ct);
}
```

---

#### RabbitMqRegistrationExtensions

Расширения для регистрации клиентов и контроллеров RabbitMQ в DI. Автоматически добавляет Health Check для подключения.

```csharp
// Регистрация клиента:
services.AddRabbitMqClient<INotificationClient, NotificationClient>(
    config: configuration.GetRabbitMqConfiguration("NotificationRabbit"));

// Регистрация SmartController (сервер):
services.AddRabbitMqController<MyRabbitController>(
    configuration: configuration.GetRabbitMqConfiguration("MyRabbit"));
```

---

#### SimpleRabbitMqConfiguration

Упрощённый POCO для конфигурации RabbitMQ из `appsettings.json`. Вместо подробной конфигурации через
`RabbitMQConfigurationBuilder` — достаточно задать несколько строковых полей.

```json
{
    "NotificationRabbit": {
        "ConnectionString": "amqp://user:pass@localhost:5672/",
        "ExName": "notifications",
        "ExType": "Fanout",
        "QueueName": "email-queue"
    }
}
```

```csharp
// Чтение конфигурации:
var config = configuration.GetRabbitMqConfiguration("NotificationRabbit");
```

---

#### RabbitMqExtensions

Расширения `IConfiguration` для чтения `RabbitMQConfiguration`. Поддерживает как формат `SimpleRabbitMqConfiguration` (
упрощённый), так и полный формат с явными секциями `ExchangeConfiguration`, `QueueConfiguration`, etc.

```csharp
RabbitMQConfiguration config = configuration.GetRabbitMqConfiguration("MyRabbitSection");
```

---

### Планировщик задач

#### SchedulingExtensions

Обёртка над [Quartz.NET](https://www.quartz-scheduler.net/) для удобного планирования фоновых задач. Задачи запускаются
до старта HTTP-сервера (через `IBeforeHostingStartedService`).

```csharp
// Регистрация планировщика:
services.AddSchedulingServices(
    waitForJobsToComplete: true,
    configureSchedulerJobs: async schedulerFactory =>
    {
        // По интервалу (extension-метод):
        await schedulerFactory.ScheduleJobAsync<CleanupJob>(
            jobIdentity: "cleanup",
            interval: TimeSpan.FromHours(1),
            delay: TimeSpan.FromSeconds(10));

        // По cron-выражению (статический метод):
        await SchedulingExtensions.ScheduleJobAsync<ReportJob>(
            schedulerFactory,
            jobIdentity: "daily-report",
            cronExpression: "0 0 8 * * ?"); // каждый день в 08:00
    });

// Реализация задачи (Quartz IJob):
public class CleanupJob(ILogger<CleanupJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Запуск задачи очистки...");
        // логика
    }
}
```

---

### Сервисы до запуска хоста

#### IBeforeHostingStartedService

Интерфейс для выполнения произвольных действий **до** старта HTTP-сервера, но **после** построения DI-контейнера. Все
зарегистрированные реализации запускаются последовательно внутри `ConfiguredApp.BuildAndRunAsync()`.

```csharp
// Реализация:
public class SeedDataService(AppDbContext db) : IBeforeHostingStartedService
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!await db.Roles.AnyAsync(cancellationToken))
        {
            db.Roles.Add(new Role("admin"));
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}

// Регистрация:
services.AddBeforeHostingStarted<SeedDataService>();
```

---

### Service Controller

`ServiceController` — встроенный контроллер, автоматически подключаемый через `ExtraStartupBase`. Предоставляет
системные эндпоинты под маршрутом `/service`:

| Эндпоинт                      | Описание                                  |
|-------------------------------|-------------------------------------------|
| `GET /service/ping`           | Возвращает `"pong"`                       |
| `GET /service/healthz`        | Health check без тела ответа (200 / 500)  |
| `GET /service/healthz-status` | Health check с детальным `UIHealthReport` |
| `GET /service/metrics`        | Prometheus-метрики в текстовом формате    |

Используется `ServiceSystemClient` для обращения к этим эндпоинтам из другого сервиса.

---

### Модели

#### ServiceApiInfo

Метаданные сервиса, передаваемые в Swagger и используемые в `ServiceController`/`ServiceSystemClient`.

```csharp
// В ExtraStartupBase:
protected override ServiceApiInfo ServiceApiInfo { get; init; } =
    new ServiceApiInfo(
        ServiceName: "OrderService",
        BaseAddress: "/order",      // PathString — base path сервиса
        Description: "Управление заказами");
```

---

#### ProblemDetailsResult

`IActionResult` для возврата `ProblemDetails` с правильным Content-Type (`application/problem+json`) и HTTP-статусом из
объекта.

```csharp
// Используется внутри ApiExceptionFilter.
// При необходимости — напрямую в контроллере:
return new ProblemDetailsResult(new ProblemDetails
{
    Status = StatusCodes.Status400BadRequest,
    Title = "Неверный запрос",
    Detail = "Поле 'name' обязательно"
});
```

---

### Swagger

#### ConfigureSwaggerOptions

Настраивает Swagger-документацию для всех версий API (`IApiVersionDescriptionProvider`). Берёт название и описание из
`ServiceApiInfo`. Автоматически помечает устаревшие версии.

```csharp
// Переопределение OpenApiInfo (опционально):
ConfigureSwaggerOptions.ApiInfo = new OpenApiInfo
{
    Title = "My Custom API",
    Version = "v2",
    Contact = new OpenApiContact { Email = "dev@example.com" }
};
```

---

### Расширения

#### EbObjectExtensions

Набор extension-методов для общих операций.

**Сериализация:**

```csharp
var obj = new { Id = 1, Name = "Test" };

// Читабельный JSON с enum как строками:
string diagnosticJson = obj.ToDiagnosticJson();

// Компактный JSON:
string json = obj.ToJson();
```

**Строки:**

```csharp
string? val = null;
val.IsNullOrEmpty();      // true
val.IsNullOrWhiteSpace(); // true

new[] { "a", "b", "c" }.Join(", "); // "a, b, c"
```

**Числовые приведения:**

```csharp
int i = 42;
uint u = i.ToUInt();

long l = 42L;
ulong ul = l.ToULong();
```

---

#### EbServerExtensions

Расширения для серверной логики.

**`ProblemDetails.CreateFromResponse(HttpResponse)`** — создаёт `ProblemDetails` из текущего HTTP-ответа:

```csharp
var pd = ProblemDetails.CreateFromResponse(context.Response);
```

**`Task.WaitUntilAsync(predicate, timeout)`** — ожидает выполнения условия с таймаутом (polling с интервалом 50 мс):

```csharp
await Task.WaitUntilAsync(
    predicate: ct => _isReady || ct.IsCancellationRequested,
    timeout: TimeSpan.FromSeconds(30));
```

---

#### EbIServiceCollectionExtensions

Расширения `IServiceCollection` для регистрации инфраструктурных компонентов:

```csharp
// Регистрация FluentValidation-валидатора:
services.AddValidator<CreateOrderValidator, CreateOrderRequest>();

// Регистрация команд:
services.AddScopedCommand<CreateOrderCommand, CreateOrderContext, Order>();
services.AddSingletonCommand<CacheWarmupCommand, Unit, Unit>();
services.AddTransientCommand<SendEmailCommand, EmailContext, bool>();

// Регистрация JwtValidator:
services.AddJwtValidator();

// Регистрация атомарных генераторов:
services.AddAtomicGenerators();
```

---

## Тестовая инфраструктура

Для тестирования приложений, построенных на базе `EBCEYS.Infrastructure`, предназначена отдельная вспомогательная
библиотека **Ebceys.Tests.Infrastructure**.

Она предоставляет:

- генератор случайных тестовых данных (`EbRandomizer`)
- утилиты для работы с портами и временем (`PortSelector`, `StopWatchElapser`)
- базовые классы для интеграционных тестов (`TestWebApplicationFactory`, `ServiceTestContext`, `ClientTestContext`)
- маршрутизацию HTTP между тестовыми серверами (`RoutingMessageHandler`)
- запуск внешних зависимостей через Testcontainers (`PostgresInitializer`, `RabbitMqInitializer`)

Подробная документация: [Ebceys.Tests.Infrastructure/README.md](Ebceys.Tests.Infrastructure/README.md)

---

## Лицензия

Этот проект лицензируется в соответствии с условиями, указанными в файле [LICENSE](LICENSE).

## Автор

Создано и поддерживается [EBCEYS](https://github.com/EBCEYS).

Если у вас возникли вопросы или проблемы, пожалуйста, создайте issue
в [репозитории GitHub](https://github.com/EBCEYS/EBCEYS.Infrastructure/issues).

---
