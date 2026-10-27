using Ebceys.Infrastructure;
using ShedulerService;

await WebApplicationBase<Startup>
    .Create(args)
    .Build(args)
    .RunAsync();