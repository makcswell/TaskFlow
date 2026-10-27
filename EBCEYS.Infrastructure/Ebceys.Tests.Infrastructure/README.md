# Ebceys.Tests.Infrastructure

Вспомогательная библиотека для тестирования ASP.NET Core приложений, построенных на базе `EBCEYS.Infrastructure`.
Предоставляет утилиты для интеграционных тестов, генерацию тестовых данных и поддержку внешних зависимостей через
Testcontainers.

---

## Навигация

- [Ebceys.Tests.Infrastructure](#ebceystestsinfrastructure)
    - [Навигация](#навигация)
    - [Установка](#установка)
    - [Компоненты](#компоненты)
        - [Helpers](#helpers)
            - [EbRandomizer](#ebrandomizer)
            - [PortSelector](#portselector)
            - [StopWatchElapser](#stopwatchelapser)
            - [EbTestCaseSource](#ebtestcasesource)
        - [Интеграционные тесты](#интеграционные-тесты)
            - [TestWebApplicationFactory](#testwebapplicationfactory)
            - [ServiceTestContext](#servicetestcontext)
            - [ClientTestContext](#clienttestcontext)
            - [TestClientInitializeOptions](#testclientinitializeoptions)
            - [RoutingMessageHandler и RoutingMessageHandlerConfiguration](#routingmessagehandler-и-routingmessagehandlerconfiguration)
            - [TestRoutingMessageHandler](#testroutingmessagehandler)
            - [TestsExtensions](#testsextensions)
        - [Внешние зависимости (Testcontainers)](#внешние-зависимости-testcontainers)
            - [IDependencyInitializer](#idependencyinitializer)
            - [PostgresInitializer](#postgressinitializer)
            - [RabbitMqInitializer](#rabbitmqinitializer)

---

## Установка

Добавьте ссылку на проект:

```bash
dotnet add reference ./Ebceys.Tests.Infrastructure.csproj
```

---

## Компоненты

---

### Helpers

#### EbRandomizer

`EbRandomizer` — многофункциональный генератор случайных данных для тестов. Поддерживает детерминированную генерацию
через `seed`.

```csharp
// Создание с фиксированным seed (воспроизводимые данные)
var rand = new EbRandomizer(seed: 42);

// Создание с случайным seed
var rand = EbRandomizer.Create();
```

**Генерация примитивных типов:**

| Метод                 | Описание                                        |
|-----------------------|-------------------------------------------------|
| `String(length?)`     | Случайная строка из букв и цифр                 |
| `HexString(length?)`  | Случайная hex-строка                            |
| `Int(min?, max?)`     | Случайное `int`                                 |
| `UInt(min?, max?)`    | Случайное `uint`                                |
| `Long(min?, max?)`    | Случайное `long`                                |
| `Byte(min?, max?)`    | Случайный `byte`                                |
| `Double(min?, max?)`  | Случайное `double`                              |
| `Decimal(min?, max?)` | Случайное `decimal`                             |
| `Bool(weight?)`       | Случайный `bool` (с настраиваемой вероятностью) |

**Генерация массивов:**

```csharp
// Массив строк
string[] strings = rand.StringArray(length: 10);

// Типизированный массив с произвольной логикой
MyDto[] items = rand.Array((i, r) => new MyDto { Id = i, Name = r.String() }, length: 5);
```

**Генерация дат:**

```csharp
DateTime dt = rand.DateTime();
DateTimeOffset dto = rand.DateTimeOffset();
TimeSpan ts = rand.TimeSpan();
```

**Генерация строк специального формата:**

```csharp
string email = rand.Email();               // "abc123@domain.com"
string domain = rand.Domain();             // "xyzabc.com"
string email2 = rand.Email(domain: "my.org"); // с указанным доменом
```

**Работа с коллекциями и перечислениями:**

```csharp
// Случайный элемент коллекции
var item = rand.RandomElement(myList);

// Несколько случайных элементов
var items = rand.RandomElements(myList, count: 3);

// Случайное значение enum
var status = rand.Enum<OrderStatus>();
```

**Генерация JWT:**

```csharp
// Полностью случайный JWT
string jwt = rand.Jwt();

// JWT с конкретными claims
string jwt = rand.Jwt(new Claim("role", "admin"), new Claim("sub", "123"));

// JWT с заранее подготовленными JwtOptions
var opts = rand.JwtOptions();
string jwt = rand.Jwt(opts);

// Доступ к IJwtGenerator с случайными параметрами
IJwtGenerator generator = rand.RandomJwtGeneratorInstance;
```

---

#### PortSelector

`PortSelector` — статический утилитный класс для поиска свободных TCP-портов.

```csharp
// Получить свободный порт (начиная с случайного)
int port = PortSelector.GetPort();

// Получить свободный порт начиная с указанного
int port = PortSelector.GetPort(port: 8080);

// Проверить, свободен ли порт
bool free = PortSelector.IsFree(5432);
```

---

#### StopWatchElapser

`StopWatchElapser` — `IDisposable`-обёртка над `Stopwatch`. Запускает таймер при создании и вызывает переданный
`Action<TimeSpan>` при вызове `Dispose`.

```csharp
using (StopWatchElapser.Create(elapsed =>
{
    _logger.LogInformation("Operation took: {Elapsed}", elapsed);
}))
{
    // код, время которого нужно измерить
    await DoSomethingAsync();
}
```

---

#### EbTestCaseSource

`EbTestCaseSource<TData>` — абстрактный базовый класс для организации параметризованных тестовых данных в NUnit.
Разделяет данные на валидные и невалидные наборы.

**Создание источника данных:**

```csharp
public class CreateOrderRequestSource : EbTestCaseSource<CreateOrderRequest>
{
    public override CreateOrderRequest SetupValidBase() =>
        new() { Amount = 100, CustomerId = Guid.NewGuid() };

    public override CreateOrderRequest? SetupInvalidBase() =>
        new() { Amount = -1, CustomerId = Guid.Empty };

    public override IEnumerable<(Func<CreateOrderRequest?, CreateOrderRequest?>, string)> GetValidCases()
    {
        yield return (base => base! with { Amount = 1 }, "Min_Amount");
        yield return (base => base! with { Amount = 9999 }, "Max_Amount");
    }

    public override IEnumerable<(Func<CreateOrderRequest?, CreateOrderRequest?>, string)> GetInvalidCases()
    {
        yield return (base => base! with { Amount = 0 }, "Zero_Amount");
        yield return (_ => null, "Null_Request");
    }
}
```

**Использование в тестах через NUnit:**

```csharp
[TestCaseSource(typeof(EbTestDataValidExecutor<CreateOrderRequestSource, CreateOrderRequest>))]
public void Validate_ShouldBeValid(CreateOrderRequest? request)
{
    var result = _validator.Validate(request!);
    result.IsValid.Should().BeTrue();
}

[TestCaseSource(typeof(EbTestDataInvalidExecutor<CreateOrderRequestSource, CreateOrderRequest>))]
public void Validate_ShouldBeInvalid(CreateOrderRequest? request)
{
    var result = _validator.Validate(request ?? new());
    result.IsValid.Should().BeFalse();
}
```

---

### Интеграционные тесты

#### TestWebApplicationFactory

`TestWebApplicationFactory<TStartup>` — расширение `WebApplicationFactory<TStartup>` с поддержкой настроек и запуска
`IBeforeHostingStartedService`. Использует `IHostBuilder` внутри для поддержки классических Startup-классов (
`UseStartup<TStartup>()`).

**Переопределяемые члены:**

| Член                                           | Описание                                            |
|------------------------------------------------|-----------------------------------------------------|
| `ServiceId` _(abstract)_                       | Идентификатор сервиса, подставляется в конфигурацию |
| `UseProductionAppSettings`                     | Загружать ли `appsettings.json` из проекта          |
| `ConfigureTestServices(services)` _(abstract)_ | Регистрация тестовых сервисов / моков               |
| `SetConfiguration(configurationBuilder)`       | Дополнительные источники конфигурации               |

```csharp
public class MyWebApplicationFactory : TestWebApplicationFactory<Startup>
{
    public override string ServiceId => "my-service-tests";

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        // Заменяем реальный сервис на мок
        services.RemoveAll<IExternalApiClient>();
        services.AddSingleton<IExternalApiClient, FakeExternalApiClient>();
    }
}
```

---

#### ServiceTestContext

`ServiceTestContext<TEntrypoint>` — базовый контекст для интеграционных тестов сервиса. Управляет жизненным циклом
`WebApplicationFactory`.

```csharp
public class MyServiceTests
{
    private readonly MyServiceTestContext _context = new();

    [OneTimeSetUp]
    public void Setup()
    {
        _context.Initialize();
    }

    [OneTimeTearDown]
    public void Teardown()
    {
        _context.Teardown();
    }
}
```

**Члены:**

| Член                                         | Описание                          |
|----------------------------------------------|-----------------------------------|
| `Factory`                                    | Экземпляр `WebApplicationFactory` |
| `BaseAddress`                                | Базовый URL тестового сервера     |
| `Initialize(configurator?)`                  | Инициализация контекста           |
| `Teardown()`                                 | Остановка и очистка               |
| `CreateWebApplicationFactory()` _(abstract)_ | Создание фабрики                  |

---

#### ClientTestContext

`ClientTestContext<TClient, TEntrypoint>` — расширение `ServiceTestContext` с автоматическим созданием типизированного
клиента.

```csharp
public class OrderServiceTestContext
    : ClientTestContext<IOrderServiceClient, Startup>
{
    protected override TestWebApplicationFactory<Startup> CreateWebApplicationFactory()
        => new MyWebApplicationFactory();

    protected override IOrderServiceClient CreateServiceClient(string baseAddress)
        => new OrderServiceClient(baseAddress);
}
```

**Дополнительные члены:**

| Член                                        | Описание                                       |
|---------------------------------------------|------------------------------------------------|
| `ServiceClient`                             | Созданный экземпляр клиента `TClient`          |
| `Services`                                  | `IServiceProvider` тестового приложения        |
| `CreateServiceSystemClient(loggerFactory?)` | Создаёт `IServiceSystemClient`                 |
| `CreateFlurlCache()`                        | Создаёт `IFlurlClientCache` с middleware из DI |

---

#### TestClientInitializeOptions

Параметры инициализации тестового клиента, передаются в `ServiceTestContext.Initialize()`.

| Свойство                   | Тип                        | Описание                                                |
|----------------------------|----------------------------|---------------------------------------------------------|
| `BaseAddress`              | `Uri?`                     | Базовый адрес (по умолчанию — случайный свободный порт) |
| `SolutionRelativePath`     | `string?`                  | Относительный путь к контент-руту проекта               |
| `BuilderConfiguration`     | `Action<IWebHostBuilder>?` | Дополнительная конфигурация хоста                       |
| `UseProductionAppSettings` | `bool`                     | Использовать ли `appsettings.json`                      |

```csharp
_context.Initialize(opts =>
{
    opts.SolutionRelativePath = "Ebceys.Infrastructure.TestApplication";
    opts.UseProductionAppSettings = false;
    opts.BuilderConfiguration = b => b.AddInMemoryConfig("Feature:Enabled", "true");
});
```

---

#### RoutingMessageHandler и RoutingMessageHandlerConfiguration

`RoutingMessageHandler` — `DelegatingHandler`, который перехватывает исходящие HTTP-запросы и перенаправляет их к
соответствующим тестовым серверам по схеме `host:port`. Используется для организации межсервисного взаимодействия в
интеграционных тестах без реальной сети.

`RoutingMessageHandlerConfiguration` хранит таблицу маршрутов `host → HttpMessageHandler`.

```csharp
// Маршрутизация запросов к другому тестовому контексту
configuration.AddRoute<IOrderClient, OrderStartup>(orderTestContext);

// Или вручную
configuration.AddRoute("http://payments-service:80/", () => paymentsFactory.Server.CreateHandler());
```

Регистрируется автоматически внутри `ServiceTestContext.Initialize()`.

---

#### TestRoutingMessageHandler

`TestRoutingMessageHandler` — альтернативный `DelegatingHandler` с глобальной статической конфигурацией маршрутов через
`RouteConfiguration`.

```csharp
// Настройка до инициализации теста
TestRoutingMessageHandler.RouteConfiguration.AddRoute(
    "http://auth-service:80/",
    () => authFactory.Server.CreateHandler()
);
```

> Предпочитайте `RoutingMessageHandler` (instanced-конфигурация), если тесты выполняются параллельно.

---

#### TestsExtensions

Расширения для `IWebHostBuilder`, упрощающие добавление конфигурации из памяти.

```csharp
builder.AddInMemoryConfig("Jwt:Issuer", "test-issuer");

builder.AddInMemoryCollection(new Dictionary<string, string?>
{
    { "Jwt:Issuer", "test-issuer" },
    { "Jwt:Audience", "test-audience" },
    { "Feature:Enabled", "true" }
});
```

---

### Внешние зависимости (Testcontainers)

#### IDependencyInitializer

Интерфейс для управления жизненным циклом внешней зависимости (контейнера) в тестах.

```csharp
public interface IDependencyInitializer<TDependency>
{
    Task<TDependency> InitializeAsync(CancellationToken token = default);
    Task TeardownAsync(CancellationToken token = default);
}
```

---

#### PostgresInitializer

`PostgresInitializer` — запускает PostgreSQL-контейнер через Testcontainers и предоставляет `PostgreSqlContainer`.

```csharp
var initializer = new PostgresInitializer(
    user: "postgres",
    password: "postgres",
    database: "testdb"
);

// В OneTimeSetUp
var container = await initializer.InitializeAsync();
var connectionString = container.GetConnectionString();

// В OneTimeTearDown
await initializer.TeardownAsync();
```

Образ по умолчанию: `postgres:13.6`. Переопределяется через параметр `image`.

---

#### RabbitMqInitializer

`RabbitMqInitializer` — запускает RabbitMQ-контейнер через Testcontainers.

```csharp
var initializer = new RabbitMqInitializer(
    username: "guest",
    password: "guest"
);

// В OneTimeSetUp
var container = await initializer.InitializeAsync();
var connectionString = container.GetConnectionString();

// В OneTimeTearDown
await initializer.TeardownAsync();
```

Образ по умолчанию: `rabbitmq:4.2.3-management`. Переопределяется через параметр `image`.

**Полный пример интеграционного теста с PostgreSQL:**

```csharp
[TestFixture]
public class OrderRepositoryTests
{
    private readonly PostgresInitializer _postgres = new("postgres", "postgres", "orders_test");
    private readonly OrderServiceTestContext _context = new();

    [OneTimeSetUp]
    public async Task Setup()
    {
        var container = await _postgres.InitializeAsync();
        _context.Initialize(opts =>
        {
            opts.BuilderConfiguration = b =>
                b.AddInMemoryConfig("Db:ConnectionString", container.GetConnectionString());
        });
    }

    [OneTimeTearDown]
    public async Task Teardown()
    {
        _context.Teardown();
        await _postgres.TeardownAsync();
    }

    [Test]
    public async Task CreateOrder_Should_Persist()
    {
        var result = await _context.ServiceClient.CreateOrderAsync(new CreateOrderRequest
        {
            Amount = 100,
            CustomerId = Guid.NewGuid()
        });

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
    }
}
```
